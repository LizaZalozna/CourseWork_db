using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class TripService
{
    public async Task<List<Trip>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        return await db.Trips
            .AsNoTracking()
            .Include(t => t.Route)
            .Include(t => t.Train)
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddAsync(
        int routeId,
        int trainId,
        DateOnly departure,
        DateOnly arrival,
        CancellationToken ct = default)
    {
        if (arrival < departure)
            return (false, "Дата прибуття має бути не раніше дати відправлення");

        await using var db = new RailwayContext();

        if (!await db.Routes.AnyAsync(r => r.Id == routeId, ct))
            return (false, "Оберіть існуючий маршрут");

        if (!await db.Trains.AnyAsync(t => t.Id == trainId, ct))
            return (false, "Оберіть існуючий поїзд");

        var entity = new Trip
        {
            RouteId       = routeId,
            TrainId       = trainId,
            DepartureDate = departure,
            ArrivalDate   = arrival
        };

        db.Trips.Add(entity);
        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(
        int id,
        int routeId,
        int trainId,
        DateOnly departure,
        DateOnly arrival,
        CancellationToken ct = default)
    {
        if (arrival < departure)
            return (false, "Дата прибуття має бути не раніше дати відправлення");

        await using var db = new RailwayContext();

        var entity = await db.Trips
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Рейс не знайдено");

        entity.RouteId       = routeId;
        entity.TrainId       = trainId;
        entity.DepartureDate = departure;
        entity.ArrivalDate   = arrival;

        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.Trips
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Рейс не знайдено");

        db.Trips.Remove(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Не можна видалити рейс: є пов'язані записи");
        }
    }
}