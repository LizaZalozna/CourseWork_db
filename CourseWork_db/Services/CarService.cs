using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourseWork_db.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork_db.Services;

public class CarService
{
    public async Task<List<Car>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        return await db.Cars
            .AsNoTracking()
            .Include(c => c.Train)
            .Include(c => c.CarType)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);
    }

    public async Task<(bool Ok, string Error)> AddAsync(
        int seatsCount,
        int trainId,
        int carTypeId,
        CancellationToken ct = default)
    {
        if (seatsCount <= 0)
            return (false, "Кількість місць має бути > 0");

        await using var db = new RailwayContext();

        var trainExists = await db.Trains.AnyAsync(t => t.Id == trainId, ct);
        if (!trainExists)
            return (false, "Оберіть існуючий поїзд");

        var carTypeExists = await db.CarTypes.AnyAsync(t => t.Id == carTypeId, ct);
        if (!carTypeExists)
            return (false, "Оберіть існуючий тип вагона");

        var carNumber = await db.Cars
            .Where(c => c.TrainId == trainId)
            .CountAsync(ct) + 1;

        var car = new Car
        {
            SeatsCount = seatsCount,
            TrainId = trainId,
            CarTypeId = carTypeId,
            CarNumber = carNumber
        };

        db.Cars.Add(car);
        await db.SaveChangesAsync(ct);

        var seats = new List<Seat>();

        for (int i = 1; i <= seatsCount; i++)
        {
            seats.Add(new Seat
            {
                CarId = car.Id,
                SeatNumber = i,
                IsWindow = (i % 4 == 1 || i % 4 == 0),
                IsUpper = (i % 2 == 0)
            });
        }

        db.Seats.AddRange(seats);
        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> UpdateAsync(
        int id,
        int seatsCount,
        int trainId,
        int carTypeId,
        CancellationToken ct = default)
    {
        if (seatsCount <= 0)
            return (false, "Кількість місць має бути > 0");

        await using var db = new RailwayContext();

        var entity = await db.Cars.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity == null)
            return (false, "Вагон не знайдено");

        entity.SeatsCount = seatsCount;
        entity.TrainId = trainId;
        entity.CarTypeId = carTypeId;

        await db.SaveChangesAsync(ct);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        await using var db = new RailwayContext();

        var car = await db.Cars.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (car == null)
            return (false, "Вагон не знайдено");

        var seats = await db.Seats
            .Where(s => s.CarId == id)
            .ToListAsync(ct);

        db.Seats.RemoveRange(seats);
        db.Cars.Remove(car);

        try
        {
            await db.SaveChangesAsync(ct);
            return (true, "");
        }
        catch (DbUpdateException)
        {
            return (false, "Не можна видалити вагон");
        }
    }
}
