using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class StationService
{
    public async Task<List<Station>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        return await db.Stations
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddAsync(
        string name,
        string city,
        string country,
        CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        city = (city ?? "").Trim();
        country = (country ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву станції");

        if (string.IsNullOrWhiteSpace(city))
            return (false, "Введіть місто");

        if (string.IsNullOrWhiteSpace(country))
            return (false, "Введіть країну");

        await using var db = new RailwayContext();

        var exists = await db.Stations.AnyAsync(
            s => s.Name == name &&
                 s.City == city &&
                 s.Country == country,
            ct);

        if (exists)
            return (false, "Така станція вже існує");

        var station = new Station
        {
            Name = name,
            City = city,
            Country = country
        };

        db.Stations.Add(station);

        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(
        int id,
        string name,
        string city,
        string country,
        CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        city = (city ?? "").Trim();
        country = (country ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Введіть назву станції");

        if (string.IsNullOrWhiteSpace(city))
            return (false, "Введіть місто");

        if (string.IsNullOrWhiteSpace(country))
            return (false, "Введіть країну");

        await using var db = new RailwayContext();

        var entity = await db.Stations
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Станцію не знайдено");

        var duplicate = await db.Stations.AnyAsync(
            s => s.Name == name &&
                 s.City == city &&
                 s.Country == country &&
                 s.Id != id,
            ct);

        if (duplicate)
            return (false, "Така станція вже існує");

        entity.Name = name;
        entity.City = city;
        entity.Country = country;

        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var entity = await db.Stations
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Станцію не знайдено");

        db.Stations.Remove(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Не можна видалити станцію: є пов'язані записи");
        }
    }
}