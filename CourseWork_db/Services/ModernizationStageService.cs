using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class ModernizationStageService
{
    public async Task<List<ModernizationStage>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();
        return await db.ModernizationStages
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

        if (await db.ModernizationStages.AnyAsync(x => x.Name == name, ct))
            return (false, "Такий етап вже існує");

        db.ModernizationStages.Add(new ModernizationStage { Name = name });
        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(int id, string name, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву");

        await using var db = new RailwayContext();

        var entity = await db.ModernizationStages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return (false, "Не знайдено");

        if (await db.ModernizationStages.AnyAsync(x => x.Name == name && x.Id != id, ct))
            return (false, "Такий етап вже існує");

        entity.Name = name;
        await db.SaveChangesAsync(ct);
        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.ModernizationStages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null)
            return (false, "Не знайдено");

        db.ModernizationStages.Remove(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Не можна видалити: є пов'язані типи вагонів");
        }
    }
}
