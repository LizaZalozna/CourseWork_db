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

    public record Segment(int FromIndex, int ToIndex);

    public static int ComputePriorityId(
        List<Segment> seatTickets, int stationCount,
        int lowPId, int midPId, int highPId)
    {
        if (seatTickets.Count == 0)
            return lowPId;

        bool completes100 = CoversFullRoute(seatTickets, stationCount)
                         || CanCompleteWithOneGap(seatTickets, stationCount);

        int occupiedSegments = CountOccupiedSegments(seatTickets);
        int totalSegments = stationCount - 1;
        bool thresholdReached = totalSegments > 0 && occupiedSegments * 100 / totalSegments >= 70;

        if (completes100 || thresholdReached)
            return highPId;

        return midPId;
    }

    public static bool WouldCompleteTo100(List<Segment> segments, int stationCount) =>
        CoversFullRoute(segments, stationCount) || CanCompleteWithOneGap(segments, stationCount);

    private static int CountOccupiedSegments(List<Segment> tickets)
    {
        var covered = new HashSet<int>();
        foreach (var t in tickets)
            for (var i = t.FromIndex; i < t.ToIndex; i++)
                covered.Add(i);
        return covered.Count;
    }

    private static bool CoversFullRoute(List<Segment> tickets, int stationCount)
    {
        int covered = 0;
        foreach (var seg in tickets.OrderBy(t => t.FromIndex))
        {
            if (seg.FromIndex > covered) break;
            covered = Math.Max(covered, seg.ToIndex);
        }
        return covered >= stationCount - 1;
    }

    private static bool CanCompleteWithOneGap(List<Segment> tickets, int stationCount)
    {
        var segments = tickets.OrderBy(t => t.FromIndex).ToList();
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
