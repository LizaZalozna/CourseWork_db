using System;

namespace CourseWork_db.DisplayInfo;

public class TicketDisplayInfo
{
    public int      TicketId        { get; set; }
    public string   RouteName       { get; set; } = "";
    public string   TrainName       { get; set; } = "";
    public string   FromStationName { get; set; } = "";
    public string   ToStationName   { get; set; } = "";
    public DateOnly DepartureDate   { get; set; }
    public string   CarTypeName     { get; set; } = "";
    public int      SeatNumber      { get; set; }
    public float    Price           { get; set; }

    public override string ToString() =>
        $"#{TicketId} | {RouteName} ({TrainName}) | {DepartureDate:dd.MM.yyyy} | {FromStationName} → {ToStationName} | Місце #{SeatNumber} ({CarTypeName}) | {Price:F2} грн";
}