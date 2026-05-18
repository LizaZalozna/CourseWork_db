using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class TariffService
{
    public async Task<List<Tariff>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        return await db.Tariffs
            .AsNoTracking()
            .Include(t => t.CarType)
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddAsync(
        int carTypeId,
        float pricePerKm,
        CancellationToken ct = default)
    {
        if (pricePerKm <= 0)
            return (false, "Вартість за км має бути > 0");

        await using var db = new RailwayContext();

        var carTypeExists = await db.CarTypes
            .AnyAsync(t => t.Id == carTypeId, ct);

        if (!carTypeExists)
            return (false, "Оберіть існуючий тип вагона");

        var exists = await db.Tariffs
            .AnyAsync(t => t.CarTypeId == carTypeId, ct);

        if (exists)
            return (false, "Тариф для цього типу вагона вже існує");

        var entity = new Tariff
        {
            CarTypeId = carTypeId,
            PricePerKm = pricePerKm
        };

        db.Tariffs.Add(entity);
        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(
        int id,
        int carTypeId,
        float pricePerKm,
        CancellationToken ct = default)
    {
        if (pricePerKm <= 0)
            return (false, "Вартість за км має бути > 0");

        await using var db = new RailwayContext();

        var entity = await db.Tariffs
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Тариф не знайдено");

        var carTypeExists = await db.CarTypes
            .AnyAsync(t => t.Id == carTypeId, ct);

        if (!carTypeExists)
            return (false, "Оберіть існуючий тип вагона");

        var duplicate = await db.Tariffs
            .AnyAsync(t => t.CarTypeId == carTypeId && t.Id != id, ct);

        if (duplicate)
            return (false, "Такий тариф для типу вагона вже існує");

        entity.CarTypeId = carTypeId;
        entity.PricePerKm = pricePerKm;

        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.Tariffs
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Тариф не знайдено");

        db.Tariffs.Remove(entity);

        await db.SaveChangesAsync(ct);

        return (true, "");
    }
}