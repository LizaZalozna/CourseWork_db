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
                ArrivalDate     = trip.ArrivalDate,
                FromStationName = stationFrom?.Name ?? "Невідома",
                ToStationName   = stationTo?.Name   ?? "Невідома",
                FromStopOrder   = fromRs.StopOrder,
                ToStopOrder     = toRs.StopOrder,
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

        var fromOrder     = fromRs.StopOrder;
        var toOrder       = toRs.StopOrder;
        var totalStations = routeStations.Count;

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
            .Where(s => s.Car != null && carIds.Contains(s.CarId))
            .ToListAsync(ct);

        if (allSeats.Count == 0)
            return (new List<SeatDisplayInfo>(), "Для вагонів не створені місця");

        var segments = await db.RouteSegments
            .AsNoTracking()
            .Where(rs => rs.RouteId == trip.RouteId)
            .ToListAsync(ct);

        var segmentOrders = segments.ToDictionary(
            s => s.Id,
            s => (
                FromOrder: routeStations.FirstOrDefault(rs => rs.StationId == s.FromStationId)?.StopOrder ?? 0,
                ToOrder:   routeStations.FirstOrDefault(rs => rs.StationId == s.ToStationId)?.StopOrder   ?? 0
            ));

        var segmentDistance = segments
            .Where(s => segmentOrders[s.Id].FromOrder >= fromOrder && segmentOrders[s.Id].ToOrder <= toOrder)
            .Sum(s => s.Distance);

        var allTickets = await db.Tickets
            .AsNoTracking()
            .Where(t => t.TripId == tripId)
            .Select(t => new { t.SeatId, t.FromStationId, t.ToStationId })
            .ToListAsync(ct);

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

            var tariff = await db.Tariffs
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.CarTypeId == seat.Car!.CarTypeId, ct);

            var pricePerKm = tariff?.PricePerKm ?? 0f;
            
            var ticketOrders = seatTickets
                .Select(t => (
                    FromOrder: routeStations.FirstOrDefault(rs => rs.StationId == t.FromStationId)?.StopOrder ?? 0,
                    ToOrder:   routeStations.FirstOrDefault(rs => rs.StationId == t.ToStationId)?.StopOrder   ?? 0
                ))
                .ToList();

            var (finalPrice, priceInfo) = _pricing.Calculate(
                segmentDistance, pricePerKm,
                fromOrder, toOrder, totalStations,
                ticketOrders);

            result.Add(new SeatDisplayInfo
            {
                SeatId      = seat.Id,
                SeatNumber  = seat.SeatNumber,
                CarId       = seat.CarId,
                CarNumber   = seat.Car?.CarNumber ?? 0,
                CarTypeName = seat.Car?.CarType?.Name ?? "Невідомий",
                IsWindow    = seat.IsWindow ?? false,
                IsUpper     = seat.IsUpper  ?? false,
                Price       = finalPrice,
                PriceInfo   = priceInfo
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

        var fromOrder     = fromRs.StopOrder;
        var toOrder       = toRs.StopOrder;
        var totalStations = routeStations.Count;

        var segments = await db.RouteSegments
            .AsNoTracking()
            .Where(rs => rs.RouteId == trip.RouteId)
            .ToListAsync(ct);

        var segmentOrders = segments.ToDictionary(
            s => s.Id,
            s => (
                FromOrder: routeStations.FirstOrDefault(rs => rs.StationId == s.FromStationId)?.StopOrder ?? 0,
                ToOrder:   routeStations.FirstOrDefault(rs => rs.StationId == s.ToStationId)?.StopOrder   ?? 0
            ));

        var segmentDistance = segments
            .Where(s => segmentOrders[s.Id].FromOrder >= fromOrder && segmentOrders[s.Id].ToOrder <= toOrder)
            .Sum(s => s.Distance);

        var tariff = await db.Tariffs
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.CarTypeId == seat.Car!.CarTypeId, ct);

        var pricePerKm = tariff?.PricePerKm ?? 0f;

        var allTickets = await db.Tickets
            .AsNoTracking()
            .Where(t => t.TripId == tripId && t.SeatId == seatId)
            .Select(t => new { t.FromStationId, t.ToStationId })
            .ToListAsync(ct);

        var ticketOrders = allTickets
            .Select(t => (
                FromOrder: routeStations.FirstOrDefault(rs => rs.StationId == t.FromStationId)?.StopOrder ?? 0,
                ToOrder:   routeStations.FirstOrDefault(rs => rs.StationId == t.ToStationId)?.StopOrder   ?? 0
            ))
            .ToList();

        var (finalPrice, priceInfo) = _pricing.CalculateDetailed(
            segmentDistance, pricePerKm,
            fromOrder, toOrder, totalStations,
            ticketOrders);

        return (true, "", finalPrice, priceInfo);
    }

    public async Task<(bool Success, string Error)> BuyTicketAsync(
        int passengerId,
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
            PassengerId   = passengerId,
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
        int passengerId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var tickets = await db.Tickets
            .AsNoTracking()
            .Include(t => t.Trip).ThenInclude(tr => tr!.Route)
            .Include(t => t.Trip).ThenInclude(tr => tr!.Train)
            .Include(t => t.FromStation)
            .Include(t => t.ToStation)
            .Include(t => t.Seat).ThenInclude(s => s!.Car).ThenInclude(c => c!.CarType)
            .Where(t => t.PassengerId == passengerId)
            .OrderByDescending(t => t.Id)
            .ToListAsync(ct);

        return tickets.Select(t => new TicketDisplayInfo
        {
            TicketId        = t.Id,
            RouteName       = t.Trip?.Route?.Name       ?? "Невідомий",
            TrainName       = t.Trip?.Train?.Name       ?? "Невідомий",
            FromStationName = t.FromStation?.Name       ?? "Невідома",
            ToStationName   = t.ToStation?.Name         ?? "Невідома",
            DepartureDate   = t.Trip?.DepartureDate     ?? DateOnly.MinValue,
            CarTypeName     = t.Seat?.Car?.CarType?.Name ?? "Невідомий",
            SeatNumber      = t.Seat?.SeatNumber        ?? 0,
            Price           = t.Price
        }).ToList();
    }
}