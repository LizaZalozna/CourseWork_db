using System;

namespace CourseWork_db.DisplayInfo;

public class RouteStationDisplayInfo
{
    public string StationName { get; set; } = "";
    public string ArrivalTime { get; set; } = "---";
    public string DepartureTime { get; set; } = "---";
    public int ArrivalDayOffset { get; set; }
    public int DepartureDayOffset { get; set; }

    public override string ToString()
    {
        var arrDay = ArrivalDayOffset > 0 ? $" (+{ArrivalDayOffset})" : "";
        var depDay = DepartureDayOffset > 0 ? $" (+{DepartureDayOffset})" : "";
        return $"{StationName} | Приб: {ArrivalTime}{arrDay} | Відпр: {DepartureTime}{depDay}";
    }
}
