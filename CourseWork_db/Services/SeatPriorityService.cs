using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class SeatPriorityService
{
    public async Task<List<SeatPriority>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.SeatPriorities
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddAsync(string name, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву");

        await using var db = new RailwayContext();

        if (await db.SeatPriorities.AnyAsync(x => x.Name == name, ct))
            return (false, "Такий пріоритет вже існує");

        db.SeatPriorities.Add(new SeatPriority { Name = name });
        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(int id, string name, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву");

        await using var db = new RailwayContext();

        var entity = await db.SeatPriorities.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return (false, "Не знайдено");

        if (await db.SeatPriorities.AnyAsync(x => x.Name == name && x.Id != id, ct))
            return (false, "Такий пріоритет вже існує");

        entity.Name = name;
        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.SeatPriorities.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return (false, "Не знайдено");

        db.SeatPriorities.Remove(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Не можна видалити: є пов'язані записи");
        }
    }
    
    private record Segment(int FromIndex, int ToIndex);

    public async Task AssignPrioritiesAsync(int tripId, CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var tickets = await db.Tickets
            .Where(t => t.TripId == tripId)
            .Select(t => new
            {
                t.SeatId,
                t.FromStationId,
                t.ToStationId
            })
            .ToListAsync(ct);

        var seats = await db.Seats
            .Where(s => s.Car.Train.Trips.Any(t => t.Id == tripId))
            .ToListAsync(ct);

        var priorities = await db.SeatPriorities.ToListAsync(ct);
        var noPriority   = priorities.First(p => p.Name == "Низький");
        var midPriority  = priorities.First(p => p.Name == "Середній");
        var highPriority = priorities.First(p => p.Name == "Високий");

        var routeStations = await db.RouteStations
            .Where(rs => rs.Route.Trips.Any(t => t.Id == tripId))
            .OrderBy(rs => rs.StopOrder)
            .Select(rs => rs.StationId)
            .ToListAsync(ct);

        foreach (var seat in seats)
        {
            var seatTickets = tickets
                .Where(t => t.SeatId == seat.Id)
                .Select(t => new Segment(
                    routeStations.IndexOf(t.FromStationId),
                    routeStations.IndexOf(t.ToStationId)
                ))
                .OrderBy(t => t.FromIndex)
                .ToList();

            int priorityId = noPriority.Id;

            if (seatTickets.Any())
            {
                bool covers100   = CoversFullRoute(seatTickets, routeStations.Count);
                bool canComplete = CanCompleteRoute(seatTickets, routeStations.Count);

                if (covers100 || canComplete)
                    priorityId = highPriority.Id;
                else
                    priorityId = midPriority.Id;
            }

            var existing = await db.SeatPriorityPerTrips
                .FirstOrDefaultAsync(x => x.SeatId == seat.Id && x.TripId == tripId, ct);

            if (existing == null)
            {
                db.SeatPriorityPerTrips.Add(new SeatPriorityPerTrip
                {
                    SeatId         = seat.Id,
                    TripId         = tripId,
                    SeatPriorityId = priorityId
                });
            }
            else
            {
                existing.SeatPriorityId = priorityId;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private bool CoversFullRoute(List<Segment> tickets, int stationCount)
    {
        var segments = tickets
            .OrderBy(t => t.FromIndex)
            .ToList();

        int covered = 0;
        foreach (var seg in segments)
        {
            if (seg.FromIndex > covered) break;
            covered = Math.Max(covered, seg.ToIndex);
        }

        return covered >= stationCount - 1;
    }

    private bool CanCompleteRoute(List<Segment> tickets, int stationCount)
    {
        var segments = tickets
            .OrderBy(t => t.FromIndex)
            .ToList();

        var gaps = new List<(int From, int To)>();
        int prev = 0;

        foreach (var seg in segments)
        {
            if (seg.FromIndex > prev)
                gaps.Add((prev, seg.FromIndex));
            prev = Math.Max(prev, seg.ToIndex);
        }

        if (prev < stationCount - 1)
            gaps.Add((prev, stationCount - 1));

        return gaps.Count == 1;
    }
}
