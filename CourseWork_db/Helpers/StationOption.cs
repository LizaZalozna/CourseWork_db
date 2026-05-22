using CourseWork_db.Models;

namespace CourseWork_db.Helpers;

public sealed class StationOption(Station station)
{
    public Station Station { get; } = station;

    public override string ToString() =>
        $"{Station.Name} ({Station.City}, {Station.Country})";
}
