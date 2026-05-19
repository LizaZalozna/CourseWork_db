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

    public async Task<(bool Ok, string Error)> UpdateAsync(
        int id,
        string name,
        CancellationToken ct = default)
    {
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву маршруту");

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
        await db.SaveChangesAsync(ct);

        return (true, "");
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

        var routeSegments = await db.RouteSegments
            .Where(s => s.RouteId == id).ToListAsync(ct);

        var trips = await db.Trips
            .Where(t => t.RouteId == id).ToListAsync(ct);

        db.RouteStations.RemoveRange(routeStations);
        db.RouteSegments.RemoveRange(routeSegments);
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

    public async Task<List<RouteStation>> GetAllStationsAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.RouteStations
            .AsNoTracking()
            .Include(s => s.Route)
            .Include(s => s.Station)
            .OrderBy(s => s.Id)
            .ToListAsync(ct);
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
            ArrivalTime   = arrivalTime,
            DepartureTime = departureTime
        });

        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<List<RouteSegment>> GetAllSegmentsAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.RouteSegments
            .AsNoTracking()
            .Include(s => s.Route)
            .Include(s => s.FromStation)
            .Include(s => s.ToStation)
            .OrderBy(s => s.Id)
            .ToListAsync(ct);
    }

    public async Task<List<RouteSegment>> GetSegmentsForRouteAsync(
        int routeId,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.RouteSegments
            .AsNoTracking()
            .Include(s => s.FromStation)
            .Include(s => s.ToStation)
            .Where(s => s.RouteId == routeId)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddSegmentAsync(
        int routeId,
        int fromStationId,
        int toStationId,
        float distance,
        CancellationToken ct = default)
    {
        if (distance <= 0)
            return (false, "Дистанція має бути > 0");

        if (fromStationId == toStationId)
            return (false, "Станції не можуть бути однаковими");

        await using var db = new RailwayContext();

        if (!await db.Routes.AnyAsync(r => r.Id == routeId, ct))
            return (false, "Оберіть існуючий маршрут");

        if (!await db.Stations.AnyAsync(s => s.Id == fromStationId, ct))
            return (false, "Оберіть станцію відправлення");

        if (!await db.Stations.AnyAsync(s => s.Id == toStationId, ct))
            return (false, "Оберіть станцію прибуття");

        var duplicate = await db.RouteSegments.AnyAsync(
            s => s.RouteId      == routeId      &&
                 s.FromStationId == fromStationId &&
                 s.ToStationId   == toStationId,
            ct);

        if (duplicate)
            return (false, "Такий сегмент вже існує");

        db.RouteSegments.Add(new RouteSegment
        {
            RouteId        = routeId,
            FromStationId  = fromStationId,
            ToStationId    = toStationId,
            Distance       = distance
        });

        await db.SaveChangesAsync(ct);
        return (true, "");
    }
}