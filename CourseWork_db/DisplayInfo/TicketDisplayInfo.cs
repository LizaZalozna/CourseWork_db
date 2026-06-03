using System;

namespace CourseWork_db.DisplayInfo;

public class TicketDisplayInfo
{
    public int      TicketId        { get; set; }
    public int      TripId          { get; set; }
    public string   RouteName       { get; set; } = "";
    public string   TrainName       { get; set; } = "";
    public string   FromStationName { get; set; } = "";
    public string   ToStationName   { get; set; } = "";
    public int      FromStationId   { get; set; }
    public int      ToStationId     { get; set; }
    public DateOnly DepartureDate   { get; set; }
    public string   CarTypeName     { get; set; } = "";
    public int      SeatNumber      { get; set; }
    public float    Price           { get; set; }
    public string   Characteristics { get; set; } = "";

    public override string ToString()
    {
        var line1 = $"#{TicketId} | {RouteName} ({TrainName}) | {DepartureDate:dd.MM.yyyy} | {FromStationName} → {ToStationName} | Місце #{SeatNumber} ({CarTypeName}) | {Price:F2} грн";
        if (string.IsNullOrEmpty(Characteristics))
            return line1;
        return $"{line1}\n{Characteristics}";
    }
}