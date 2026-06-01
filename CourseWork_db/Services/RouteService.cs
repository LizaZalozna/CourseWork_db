using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class RouteService
{
    public async Task<List<Route>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.Routes
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error, int Id)> AddAsync(
        string name,
        CancellationToken ct = default)
    {
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву маршруту", 0);

        await using var db = new RailwayContext();

        var exists = await db.Routes.AnyAsync(r => r.Name == name, ct);

        if (exists)
            return (false, "Такий маршрут вже існує", 0);

        var route = new Route { Name = name };

        db.Routes.Add(route);
        await db.SaveChangesAsync(ct);

        return (true, "", route.Id);
    }

    public async Task<(bool Ok, string Error)> UpdateRouteFullAsync(
        int id,
        string name,
        List<(int StationId, int StopOrder, int DayOffset, TimeOnly ArrivalTime, TimeOnly DepartureTime, float Distance)> stations,
        CancellationToken ct = default)
    {
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву маршруту");

        if (stations.Count < 2)
            return (false, "Додайте мінімум 2 станції");

        await using var db = new RailwayContext();

        var entity = await db.Routes
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Маршрут не знайдено");

        var duplicate = await db.Routes.AnyAsync(
            r => r.Name == name && r.Id != id, ct);

        if (duplicate)
            return (false, "Такий маршрут вже існує");

        entity.Name = name;

        var oldStations = await db.RouteStations
            .Where(s => s.RouteId == id).ToListAsync(ct);

        db.RouteStations.RemoveRange(oldStations);

        foreach (var (stationId, stopOrder, dayOffset, arrivalTime, departureTime, _) in stations)
        {
            db.RouteStations.Add(new RouteStation
            {
                RouteId = id,
                StationId = stationId,
                StopOrder = stopOrder,
                DayOffset = dayOffset,
                ArrivalTime = arrivalTime,
                DepartureTime = departureTime
            });
        }

        for (var i = 0; i < stations.Count - 1; i++)
        {
            var dist = stations[i].Distance;
            if (dist > 0)
            {
                await EnsureSegmentInternalAsync(db, stations[i].StationId, stations[i + 1].StationId, dist, ct);
            }
        }

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Помилка при оновленні маршруту");
        }
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.Routes
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Маршрут не знайдено");

        var routeStations = await db.RouteStations
            .Where(s => s.RouteId == id).ToListAsync(ct);

        var trips = await db.Trips
            .Where(t => t.RouteId == id).ToListAsync(ct);

        db.RouteStations.RemoveRange(routeStations);
        db.Trips.RemoveRange(trips);
        db.Routes.Remove(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Не можна видалити маршрут: є пов'язані записи");
        }
    }

    public async Task<List<RouteStation>> GetStationsForRouteAsync(
        int routeId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.RouteStations
            .AsNoTracking()
            .Include(s => s.Station)
            .Where(s => s.RouteId == routeId)
            .OrderBy(s => s.StopOrder)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddStationAsync(
        int routeId,
        int stationId,
        int stopOrder,
        TimeOnly arrivalTime,
        TimeOnly departureTime,
        int dayOffset = 0,
        CancellationToken ct = default)
    {
        if (stopOrder <= 0)
            return (false, "Порядок зупинки має бути > 0");

        await using var db = new RailwayContext();

        if (!await db.Routes.AnyAsync(r => r.Id == routeId, ct))
            return (false, "Оберіть існуючий маршрут");

        if (!await db.Stations.AnyAsync(s => s.Id == stationId, ct))
            return (false, "Оберіть існуючу станцію");

        var duplicate = await db.RouteStations.AnyAsync(
            s => s.RouteId == routeId && s.StationId == stationId, ct);

        if (duplicate)
            return (false, "Ця станція вже додана до маршруту");

        db.RouteStations.Add(new RouteStation
        {
            RouteId       = routeId,
            StationId     = stationId,
            StopOrder     = stopOrder,
            DayOffset     = dayOffset,
            ArrivalTime   = arrivalTime,
            DepartureTime = departureTime
        });

        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<Segment?> GetSegmentAsync(
        int fromStationId,
        int toStationId,
        CancellationToken ct = default)
    {
        var (a, b) = fromStationId < toStationId
            ? (fromStationId, toStationId) : (toStationId, fromStationId);

        await using var db = new RailwayContext();
        return await db.Segments
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.FromStationId == a && s.ToStationId == b, ct);
    }

    public async Task<Dictionary<(int FromId, int ToId), float>> GetSegmentsForRouteAsync(
        int routeId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var stations = await db.RouteStations
            .AsNoTracking()
            .Where(s => s.RouteId == routeId)
            .OrderBy(s => s.StopOrder)
            .ToListAsync(ct);

        var stationIds = stations.Select(s => s.StationId).Distinct().ToList();

        if (stationIds.Count < 2)
            return new Dictionary<(int, int), float>();

        var segments = await db.Segments
            .AsNoTracking()
            .Where(s => stationIds.Contains(s.FromStationId) && stationIds.Contains(s.ToStationId))
            .ToListAsync(ct);

        var result = new Dictionary<(int, int), float>();
        foreach (var seg in segments)
        {
            result[(seg.FromStationId, seg.ToStationId)] = seg.Distance;
            result[(seg.ToStationId, seg.FromStationId)] = seg.Distance;
        }

        return result;
    }

    public async Task<(bool Ok, string Error)> EnsureSegmentAsync(
        int fromStationId,
        int toStationId,
        float distance,
        CancellationToken ct = default)
    {
        if (fromStationId == toStationId)
            return (false, "Станції не можуть бути однаковими");

        var (a, b) = fromStationId < toStationId
            ? (fromStationId, toStationId) : (toStationId, fromStationId);

        await using var db = new RailwayContext();

        var existing = await db.Segments
            .FirstOrDefaultAsync(s => s.FromStationId == a && s.ToStationId == b, ct);

        if (existing != null)
            return (true, "");

        if (distance <= 0)
            return (false, "Дистанція має бути > 0");

        db.Segments.Add(new Segment
        {
            FromStationId = a,
            ToStationId   = b,
            Distance      = distance
        });

        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    private static async Task EnsureSegmentInternalAsync(
        RailwayContext db,
        int fromStationId,
        int toStationId,
        float distance,
        CancellationToken ct = default)
    {
        var (a, b) = fromStationId < toStationId
            ? (fromStationId, toStationId) : (toStationId, fromStationId);

        var existing = await db.Segments
            .FirstOrDefaultAsync(s => s.FromStationId == a && s.ToStationId == b, ct);

        if (existing == null && distance > 0)
        {
            db.Segments.Add(new Segment
            {
                FromStationId = a,
                ToStationId   = b,
                Distance      = distance
            });
        }
    }
}
