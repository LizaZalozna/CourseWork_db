using System;

namespace CourseWork_db.DisplayInfo;

public class RouteStationDisplayInfo
{
    public int StopOrder { get; set; }
    public string StationName { get; set; } = "";
    public string StationInfo { get; set; } = "";
    public string ArrivalTime { get; set; } = "---";
    public string DepartureTime { get; set; } = "---";
    public int DayOffset { get; set; }
    public bool IsFromStation { get; set; }
    public bool IsToStation { get; set; }

    public override string ToString()
    {
        var day = DayOffset > 0 ? $" (+{DayOffset})" : "";
        return $"{StationName} | Приб: {ArrivalTime}{day} | Відпр: {DepartureTime}{day}";
    }
}
