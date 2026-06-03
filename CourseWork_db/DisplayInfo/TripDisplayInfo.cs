using System;

namespace CourseWork_db.DisplayInfo;

public class TripDisplayInfo
{
    public int      TripId          { get; set; }
    public string   RouteName       { get; set; } = "";
    public string   TrainName       { get; set; } = "";
    public DateOnly DepartureDate   { get; set; }
    public string   FromStationName { get; set; } = "";
    public string   ToStationName   { get; set; } = "";
    public int      FromStationId   { get; set; }
    public int      ToStationId     { get; set; }

    public override string ToString() =>
        $"{RouteName} | {TrainName} | {DepartureDate:dd.MM.yyyy} | {FromStationName} → {ToStationName}";
}