using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class CarTypeService
{
    public async Task<List<CarType>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        return await db.CarTypes
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
            return (false, "Введіть назву типу вагона");

        await using var db = new RailwayContext();

        var exists = await db.CarTypes
            .AnyAsync(x => x.Name == name, ct);

        if (exists)
            return (false, "Такий тип вагона вже існує");

        var entity = new CarType
        {
            Name = name
        };

        db.CarTypes.Add(entity);
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
            return (false, "Введіть назву типу вагона");

        await using var db = new RailwayContext();

        var entity = await db.CarTypes
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Тип вагона не знайдено");

        var duplicate = await db.CarTypes
            .AnyAsync(x => x.Name == name && x.Id != id, ct);

        if (duplicate)
            return (false, "Такий тип вагона вже існує");

        entity.Name = name;

        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.CarTypes
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Тип вагона не знайдено");

        db.CarTypes.Remove(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Не можна видалити тип вагона: є пов'язані записи");
        }
    }
}