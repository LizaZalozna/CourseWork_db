using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.DisplayInfo;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class TicketService
{
    private readonly PricingService _pricing = new();

    public async Task<List<Station>> GetStationsAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        return await db.Stations
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<List<TripDisplayInfo>> FindTripsAsync(
        int fromStationId,
        int toStationId,
        DateTime date,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var trips = await db.Trips
            .AsNoTracking()
            .Include(t => t.Route)
            .Include(t => t.Train)
            .Where(t => t.DepartureDate == DateOnly.FromDateTime(date.Date))
            .OrderBy(t => t.DepartureDate)
            .ToListAsync(ct);

        if (trips.Count == 0)
            return new List<TripDisplayInfo>();

        var routeIds = trips.Select(t => t.RouteId).Distinct().ToList();

        var routeStations = await db.RouteStations
            .AsNoTracking()
            .Where(rs => routeIds.Contains(rs.RouteId))
            .ToListAsync(ct);

        var stationFrom = await db.Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == fromStationId, ct);

        var stationTo = await db.Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == toStationId, ct);

        var result = new List<TripDisplayInfo>();

        foreach (var trip in trips)
        {
            var stations = routeStations
                .Where(rs => rs.RouteId == trip.RouteId)
                .OrderBy(rs => rs.StopOrder)
                .ToList();

            var fromRs = stations.FirstOrDefault(rs => rs.StationId == fromStationId);
            var toRs   = stations.FirstOrDefault(rs => rs.StationId == toStationId);

            if (fromRs == null || toRs == null || fromRs.StopOrder >= toRs.StopOrder)
                continue;

            result.Add(new TripDisplayInfo
            {
                TripId          = trip.Id,
                RouteName       = trip.Route?.Name ?? "Невідомий",
                TrainName       = trip.Train?.Name ?? "Невідомий",
                DepartureDate   = trip.DepartureDate,
                FromStationName = stationFrom?.Name ?? "Невідома",
                ToStationName   = stationTo?.Name   ?? "Невідома",
                FromStationId   = fromStationId,
                ToStationId     = toStationId
            });
        }

        return result;
    }

    public async Task<(List<SeatDisplayInfo> Seats, string Debug)> GetFreeSeatsAsync(
        int tripId,
        int fromStationId,
        int toStationId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip == null)
            return (new List<SeatDisplayInfo>(), "Рейс не знайдено");

        var routeStations = await db.RouteStations
            .AsNoTracking()
            .Where(rs => rs.RouteId == trip.RouteId)
            .ToListAsync(ct);

        if (routeStations.Count == 0)
            return (new List<SeatDisplayInfo>(), $"Для маршруту {trip.RouteId} не додані зупинки");

        var fromRs = routeStations.FirstOrDefault(rs => rs.StationId == fromStationId);
        var toRs   = routeStations.FirstOrDefault(rs => rs.StationId == toStationId);

        if (fromRs == null)
            return (new List<SeatDisplayInfo>(), $"Станція відправлення {fromStationId} не є зупинкою маршруту");

        if (toRs == null)
            return (new List<SeatDisplayInfo>(), $"Станція прибуття {toStationId} не є зупинкою маршруту");

        var fromOrder = fromRs.StopOrder;
        var toOrder   = toRs.StopOrder;

        var cars = await db.Cars
            .AsNoTracking()
            .Where(c => c.TrainId == trip.TrainId)
            .ToListAsync(ct);

        if (cars.Count == 0)
            return (new List<SeatDisplayInfo>(), $"Для поїзда {trip.TrainId} не додані вагони");

        var carIds = cars.Select(c => c.Id).ToList();

        var allSeats = await db.Seats
            .AsNoTracking()
            .Include(s => s.Car)
            .ThenInclude(c => c!.CarType)
            .ThenInclude(ct => ct!.CarTypeName)
            .Include(s => s.SeatCharacteristicMaps)
            .ThenInclude(scm => scm.SeatCharacteristic)
            .ThenInclude(sc => sc.CharacteristicType)
            .Where(s => s.Car != null && carIds.Contains(s.CarId))
            .ToListAsync(ct);

        if (allSeats.Count == 0)
            return (new List<SeatDisplayInfo>(), "Для вагонів не створені місця");

        var segmentDistance = await GetSegmentDistanceAsync(db, routeStations, fromOrder, toOrder, ct);

        var allTickets = await db.Tickets
            .AsNoTracking()
            .Where(t => t.TripId == tripId)
            .Select(t => new { t.SeatId, t.FromStationId, t.ToStationId })
            .ToListAsync(ct);

        var carTypeIds = cars.Select(c => c.CarTypeId).Distinct().ToList();
        var allowedChars = await db.CarTypeAllowedCharacteristics
            .AsNoTracking()
            .Include(ac => ac.SeatCharacteristic)
            .ThenInclude(sc => sc.CharacteristicType)
            .Where(ac => carTypeIds.Contains(ac.CarTypeId))
            .ToListAsync(ct);

        var allowedByCarType = allowedChars
            .GroupBy(ac => ac.CarTypeId)
            .ToDictionary(g => g.Key, g => g.Select(ac => ac.SeatCharacteristicId).ToHashSet());

        var allPriorityDefs = await db.SeatPriorities.ToListAsync(ct);
        var lowPId  = allPriorityDefs.First(p => p.Name == "Низький").Id;
        var midPId  = allPriorityDefs.First(p => p.Name == "Середній").Id;
        var highPId = allPriorityDefs.First(p => p.Name == "Високий").Id;

        var result = new List<SeatDisplayInfo>();

        foreach (var seat in allSeats)
        {
            if (seat.Car == null) continue;

            var seatTickets = allTickets.Where(t => t.SeatId == seat.Id).ToList();
            
            var isOccupied = seatTickets.Any(t =>
            {
                var tFrom = routeStations.FirstOrDefault(rs => rs.StationId == t.FromStationId)?.StopOrder ?? 0;
                var tTo   = routeStations.FirstOrDefault(rs => rs.StationId == t.ToStationId)?.StopOrder   ?? 0;
                return !(toOrder <= tFrom || fromOrder >= tTo);
            });

            if (isOccupied) continue;

            var carType = seat.Car!.CarType;
            var pricePerKm   = carType?.PricePerKm ?? 0f;
            var servicePrice = carType?.ServicePrice ?? 0f;

            var seatSegments = seatTickets
                .Select(t => new SeatPriorityService.Segment(
                    routeStations.FindIndex(rs => rs.StationId == t.FromStationId),
                    routeStations.FindIndex(rs => rs.StationId == t.ToStationId)
                ))
                .ToList();

            var realTicketCount = seatTickets.Count;

            seatSegments.Add(new SeatPriorityService.Segment(fromOrder, toOrder));

            int hypPriorityId;
            if (realTicketCount == 0)
                hypPriorityId = SeatPriorityService.WouldCompleteTo100(seatSegments, routeStations.Count)
                    ? highPId : lowPId;
            else
                hypPriorityId = SeatPriorityService.ComputePriorityId(
                    seatSegments, routeStations.Count, lowPId, midPId, highPId);

            var priorityName = allPriorityDefs.First(p => p.Id == hypPriorityId).Name;

            var chars = seat.SeatCharacteristicMaps?
                .Select(scm => scm.SeatCharacteristic)
                .Where(sc => sc?.CharacteristicType != null)
                .Where(sc =>
                {
                    if (carType?.Id == null) return true;
                    return allowedByCarType.TryGetValue(carType.Id, out var allowed)
                        && allowed.Contains(sc.Id);
                })
                .Select(sc => $"{sc.CharacteristicType.Name}: {sc.Value}")
                .ToList() ?? new();

            var (finalPrice, priceInfo) = _pricing.Calculate(
                segmentDistance, pricePerKm, servicePrice,
                priorityName);

            result.Add(new SeatDisplayInfo
            {
                SeatId          = seat.Id,
                SeatNumber      = seat.SeatNumber,
                CarNumber       = seat.Car?.CarNumber ?? 0,
                CarTypeName     = carType?.CarTypeName?.Name ?? "Невідомий",
                Price           = finalPrice,
                PriceInfo       = priceInfo,
                PriorityName    = priorityName,
                Characteristics = string.Join(", ", chars)
            });
        }

        return (result.OrderBy(r => r.Price).ToList(), $"Знайдено {result.Count} місць");
    }

    public async Task<(bool Success, string Error, float Price, string PriceInfo)> CalculatePriceAsync(
        int tripId,
        int seatId,
        int fromStationId,
        int toStationId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip == null)
            return (false, "Рейс не знайдено", 0, "");

        var seat = await db.Seats
            .AsNoTracking()
            .Include(s => s.Car)
            .ThenInclude(c => c!.CarType)
            .FirstOrDefaultAsync(s => s.Id == seatId, ct);

        if (seat == null)
            return (false, "Місце не знайдено", 0, "");

        var routeStations = await db.RouteStations
            .AsNoTracking()
            .Where(rs => rs.RouteId == trip.RouteId)
            .OrderBy(rs => rs.StopOrder)
            .ToListAsync(ct);

        var fromRs = routeStations.FirstOrDefault(rs => rs.StationId == fromStationId);
        var toRs   = routeStations.FirstOrDefault(rs => rs.StationId == toStationId);

        if (fromRs == null || toRs == null)
            return (false, "Станції не знайдені в маршруті", 0, "");

        var fromOrder = fromRs.StopOrder;
        var toOrder   = toRs.StopOrder;

        var segmentDistance = await GetSegmentDistanceAsync(db, routeStations, fromOrder, toOrder, ct);

        var carType      = seat.Car?.CarType;
        var pricePerKm   = carType?.PricePerKm ?? 0f;
        var servicePrice = carType?.ServicePrice ?? 0f;

        var allPriorityDefs = await db.SeatPriorities.ToListAsync(ct);
        var lowPId  = allPriorityDefs.First(p => p.Name == "Низький").Id;
        var midPId  = allPriorityDefs.First(p => p.Name == "Середній").Id;
        var highPId = allPriorityDefs.First(p => p.Name == "Високий").Id;

        var existingTickets = await db.Tickets
            .AsNoTracking()
            .Where(t => t.TripId == tripId && t.SeatId == seatId)
            .Select(t => new { t.FromStationId, t.ToStationId })
            .ToListAsync(ct);

        var seatSegments = existingTickets
            .Select(t => new SeatPriorityService.Segment(
                routeStations.FindIndex(rs => rs.StationId == t.FromStationId),
                routeStations.FindIndex(rs => rs.StationId == t.ToStationId)
            ))
            .ToList();

        var realTicketCount = existingTickets.Count;

        seatSegments.Add(new SeatPriorityService.Segment(fromOrder, toOrder));

        int hypPriorityId;
        if (realTicketCount == 0)
            hypPriorityId = SeatPriorityService.WouldCompleteTo100(seatSegments, routeStations.Count)
                ? highPId : lowPId;
        else
            hypPriorityId = SeatPriorityService.ComputePriorityId(
                seatSegments, routeStations.Count, lowPId, midPId, highPId);

        var priorityName = allPriorityDefs.First(p => p.Id == hypPriorityId).Name;

        var (finalPrice, priceInfo) = _pricing.Calculate(
            segmentDistance, pricePerKm, servicePrice,
            priorityName);

        return (true, "", finalPrice, priceInfo);
    }

    public async Task<(bool Success, string Error)> BuyTicketAsync(
        int userId,
        int tripId,
        int seatId,
        int fromStationId,
        int toStationId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var trip = await db.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip == null)
            return (false, "Рейс не знайдено");

        var routeStations = await db.RouteStations
            .Where(rs => rs.RouteId == trip.RouteId)
            .ToListAsync(ct);

        var fromRs = routeStations.FirstOrDefault(rs => rs.StationId == fromStationId);
        var toRs   = routeStations.FirstOrDefault(rs => rs.StationId == toStationId);

        if (fromRs == null || toRs == null)
            return (false, "Станції не знайдені в маршруті");

        if (fromRs.StopOrder >= toRs.StopOrder)
            return (false, "Станція прибуття має бути після станції відправлення");

        var fromOrder = fromRs.StopOrder;
        var toOrder   = toRs.StopOrder;

        var existingTickets = await db.Tickets
            .Where(t => t.TripId == tripId && t.SeatId == seatId)
            .Select(t => new { t.FromStationId, t.ToStationId })
            .ToListAsync(ct);

        var isOccupied = existingTickets.Any(t =>
        {
            var tFrom = routeStations.FirstOrDefault(rs => rs.StationId == t.FromStationId)?.StopOrder ?? 0;
            var tTo   = routeStations.FirstOrDefault(rs => rs.StationId == t.ToStationId)?.StopOrder   ?? 0;
            return !(toOrder <= tFrom || fromOrder >= tTo);
        });

        if (isOccupied)
            return (false, "Це місце вже зайняте на обраному відрізку");

        var (ok, _, price, _) = await CalculatePriceAsync(tripId, seatId, fromStationId, toStationId, ct);

        if (!ok)
            return (false, "Не вдалося розрахувати ціну");

        var entity = new Ticket
        {
            TripId        = tripId,
            UserId        = userId,
            SeatId        = seatId,
            FromStationId = fromStationId,
            ToStationId   = toStationId,
            Price         = price
        };

        db.Tickets.Add(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                inner.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                inner.Contains("повтор", StringComparison.OrdinalIgnoreCase))
                return (false, "Не вдалося зберегти квиток: місце вже зайняте або дублікат запису.");

            return (false, $"Не вдалося зберегти квиток: {inner}");
        }
    }

    public async Task<List<TicketDisplayInfo>> GetMyTicketsAsync(
        int userId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var tickets = await db.Tickets
            .AsNoTracking()
            .Include(t => t.Trip).ThenInclude(tr => tr!.Route)
            .Include(t => t.Trip).ThenInclude(tr => tr!.Train)
            .Include(t => t.FromStation)
            .Include(t => t.ToStation)
            .Include(t => t.Seat).ThenInclude(s => s!.Car).ThenInclude(c => c!.CarType).ThenInclude(ct => ct!.CarTypeName)
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Seat!.Car!.CarNumber)
            .ThenBy(t => t.Seat!.SeatNumber)
            .ToListAsync(ct);

        var seatIds = tickets.Select(t => t.SeatId).Distinct().ToList();
        var charMaps = await db.SeatCharacteristicMaps
            .AsNoTracking()
            .Include(scm => scm.SeatCharacteristic).ThenInclude(sc => sc.CharacteristicType)
            .Where(scm => seatIds.Contains(scm.SeatId))
            .ToListAsync(ct);
        var charBySeat = charMaps.GroupBy(scm => scm.SeatId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return tickets.Select(t =>
        {
            var maps = charBySeat.GetValueOrDefault(t.SeatId);
            var chars = maps?
                .Select(scm => scm.SeatCharacteristic)
                .Where(sc => sc?.CharacteristicType != null)
                .Select(sc => $"{sc.CharacteristicType.Name}: {sc.Value}")
                .ToList() ?? new();

            return new TicketDisplayInfo
            {
                TicketId        = t.Id,
                TripId          = t.TripId,
                RouteName       = t.Trip?.Route?.Name               ?? "Невідомий",
                TrainName       = t.Trip?.Train?.Name               ?? "Невідомий",
                FromStationName = t.FromStation?.Name               ?? "Невідома",
                ToStationName   = t.ToStation?.Name                 ?? "Невідома",
                FromStationId   = t.FromStationId,
                ToStationId     = t.ToStationId,
                DepartureDate   = t.Trip?.DepartureDate             ?? DateOnly.MinValue,
                CarTypeName     = t.Seat?.Car?.CarType?.CarTypeName?.Name ?? "Невідомий",
                SeatNumber      = t.Seat?.SeatNumber                ?? 0,
                Price           = t.Price,
                Characteristics = string.Join(", ", chars)
            };
        }).ToList();
    }

    public async Task<List<RouteStationDisplayInfo>> GetRouteStationsAsync(
        int tripId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip == null)
            return new List<RouteStationDisplayInfo>();

        var routeStations = await db.RouteStations
            .AsNoTracking()
            .Include(rs => rs.Station)
            .Where(rs => rs.RouteId == trip.RouteId)
            .OrderBy(rs => rs.StopOrder)
            .ToListAsync(ct);

        var result = new List<RouteStationDisplayInfo>();
        var prevDayOffset = 0;

        for (var i = 0; i < routeStations.Count; i++)
        {
            var rs = routeStations[i];
            var isLast = i == routeStations.Count - 1;

            result.Add(new RouteStationDisplayInfo
            {
                StationName        = rs.Station?.Name ?? "Невідома",
                ArrivalTime        = (rs.ArrivalTime ?? TimeOnly.MinValue).ToString(),
                DepartureTime      = (rs.DepartureTime ?? TimeOnly.MinValue).ToString(),
                ArrivalDayOffset   = i > 0 ? prevDayOffset : 0,
                DepartureDayOffset = !isLast ? rs.DayOffset : 0
            });

            prevDayOffset = rs.DayOffset;
        }

        return result;
    }

    private static async Task<float> GetSegmentDistanceAsync(
        RailwayContext db,
        List<RouteStation> routeStations,
        int fromOrder,
        int toOrder,
        CancellationToken ct = default)
    {
        var stationIds = routeStations.Select(rs => rs.StationId).Distinct().ToList();

        if (stationIds.Count < 2)
            return 0f;

        var segments = await db.Segments
            .AsNoTracking()
            .Where(s => stationIds.Contains(s.FromStationId) && stationIds.Contains(s.ToStationId))
            .ToListAsync(ct);

        var segmentMap = new Dictionary<(int, int), float>();
        foreach (var seg in segments)
        {
            segmentMap[(seg.FromStationId, seg.ToStationId)] = seg.Distance;
            segmentMap[(seg.ToStationId, seg.FromStationId)] = seg.Distance;
        }

        var ordered = routeStations.OrderBy(rs => rs.StopOrder).ToList();
        var total = 0f;

        for (var i = 0; i < ordered.Count - 1; i++)
        {
            var curr = ordered[i];
            var next = ordered[i + 1];
            if (curr.StopOrder >= fromOrder && next.StopOrder <= toOrder)
            {
                if (segmentMap.TryGetValue((curr.StationId, next.StationId), out var d))
                    total += d;
            }
        }

        return total;
    }
}
