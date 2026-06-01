using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class SeatCharacteristicService
{
    public async Task<List<SeatCharacteristic>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.SeatCharacteristics
            .AsNoTracking()
            .Include(x => x.CharacteristicType)
            .OrderBy(x => x.CharacteristicType!.Name)
            .ThenBy(x => x.Value)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddAsync(
        int characteristicTypeId, string value, CancellationToken ct = default)
    {
        value = (value ?? "").Trim();
        if (string.IsNullOrWhiteSpace(value))
            return (false, "Введіть значення");

        await using var db = new RailwayContext();

        if (!await db.SeatCharacteristicTypes.AnyAsync(x => x.Id == characteristicTypeId, ct))
            return (false, "Оберіть існуючий тип");

        if (await db.SeatCharacteristics.AnyAsync(
                x => x.CharacteristicTypeId == characteristicTypeId && x.Value == value, ct))
            return (false, "Така характеристика вже існує для цього типу");

        db.SeatCharacteristics.Add(new SeatCharacteristic
        {
            CharacteristicTypeId = characteristicTypeId,
            Value = value
        });
        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(
        int id, int characteristicTypeId, string value, CancellationToken ct = default)
    {
        value = (value ?? "").Trim();
        if (string.IsNullOrWhiteSpace(value))
            return (false, "Введіть значення");

        await using var db = new RailwayContext();

        var entity = await db.SeatCharacteristics.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return (false, "Не знайдено");

        if (!await db.SeatCharacteristicTypes.AnyAsync(x => x.Id == characteristicTypeId, ct))
            return (false, "Оберіть існуючий тип");

        if (await db.SeatCharacteristics.AnyAsync(
                x => x.CharacteristicTypeId == characteristicTypeId && x.Value == value && x.Id != id, ct))
            return (false, "Така характеристика вже існує для цього типу");

        entity.CharacteristicTypeId = characteristicTypeId;
        entity.Value = value;
        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.SeatCharacteristics.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return (false, "Не знайдено");

        db.SeatCharacteristics.Remove(entity);

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
