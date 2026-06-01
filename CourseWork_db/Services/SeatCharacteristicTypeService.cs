using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class SeatCharacteristicTypeService
{
    public async Task<List<SeatCharacteristicType>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.SeatCharacteristicTypes
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

        if (await db.SeatCharacteristicTypes.AnyAsync(x => x.Name == name, ct))
            return (false, "Такий тип вже існує");

        db.SeatCharacteristicTypes.Add(new SeatCharacteristicType { Name = name });
        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(int id, string name, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву");

        await using var db = new RailwayContext();

        var entity = await db.SeatCharacteristicTypes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return (false, "Не знайдено");

        if (await db.SeatCharacteristicTypes.AnyAsync(x => x.Name == name && x.Id != id, ct))
            return (false, "Такий тип вже існує");

        entity.Name = name;
        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.SeatCharacteristicTypes
            .Include(x => x.SeatCharacteristics)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return (false, "Не знайдено");

        if (entity.SeatCharacteristics.Count > 0)
            return (false, "Спочатку видаліть характеристики цього типу");

        db.SeatCharacteristicTypes.Remove(entity);

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
}
