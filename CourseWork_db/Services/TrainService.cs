using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class TrainService
{
    public async Task<List<Train>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        return await db.Trains
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddAsync(
        string name,
        CancellationToken ct = default)
    {
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву поїзда");

        await using var db = new RailwayContext();

        var exists = await db.Trains
            .AnyAsync(t => t.Name == name, ct);

        if (exists)
            return (false, "Такий поїзд вже існує");

        var train = new Train
        {
            Name = name
        };

        db.Trains.Add(train);
        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(
        int id,
        string name,
        CancellationToken ct = default)
    {
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву поїзда");

        await using var db = new RailwayContext();

        var entity = await db.Trains
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Поїзд не знайдено");

        var duplicate = await db.Trains
            .AnyAsync(t => t.Name == name && t.Id != id, ct);

        if (duplicate)
            return (false, "Такий поїзд вже існує");

        entity.Name = name;

        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.Trains
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Поїзд не знайдено");

        db.Trains.Remove(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Не можна видалити поїзд: є пов'язані записи");
        }
    }
}