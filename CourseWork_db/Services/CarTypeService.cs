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
            .Include(t => t.CarTypeName)
            .Include(t => t.ModernizationStage)
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddAsync(
        int carTypeNameId,
        int modernizationStageId,
        float pricePerKm,
        float servicePrice,
        CancellationToken ct = default)
    {
        if (pricePerKm <= 0)
            return (false, "Вартість за км має бути > 0");

        if (servicePrice <= 0)
            return (false, "Вартість обслуговування має бути > 0");

        await using var db = new RailwayContext();

        if (!await db.CarTypeNames.AnyAsync(x => x.Id == carTypeNameId, ct))
            return (false, "Оберіть існуючу назву типу вагона");

        if (!await db.ModernizationStages.AnyAsync(x => x.Id == modernizationStageId, ct))
            return (false, "Оберіть існуючий етап модернізації");

        var duplicate = await db.CarTypes.AnyAsync(x =>
            x.CarTypeNameId == carTypeNameId && x.ModernizationStageId == modernizationStageId, ct);

        if (duplicate)
            return (false, "Такий тип вагона вже існує");

        var entity = new CarType
        {
            CarTypeNameId = carTypeNameId,
            ModernizationStageId = modernizationStageId,
            PricePerKm = pricePerKm,
            ServicePrice = servicePrice
        };

        db.CarTypes.Add(entity);
        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(
        int id,
        int carTypeNameId,
        int modernizationStageId,
        float pricePerKm,
        float servicePrice,
        CancellationToken ct = default)
    {
        if (pricePerKm <= 0)
            return (false, "Вартість за км має бути > 0");

        if (servicePrice <= 0)
            return (false, "Вартість обслуговування має бути > 0");

        await using var db = new RailwayContext();

        var entity = await db.CarTypes.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Тип вагона не знайдено");

        var duplicate = await db.CarTypes.AnyAsync(x =>
            x.CarTypeNameId == carTypeNameId && x.ModernizationStageId == modernizationStageId && x.Id != id, ct);

        if (duplicate)
            return (false, "Такий тип вагона вже існує");

        entity.CarTypeNameId = carTypeNameId;
        entity.ModernizationStageId = modernizationStageId;
        entity.PricePerKm = pricePerKm;
        entity.ServicePrice = servicePrice;

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
