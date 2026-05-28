using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class RouteStation
{
    public int Id { get; set; }

    public int RouteId { get; set; }

    public int StationId { get; set; }

    public int StopOrder { get; set; }

    public int DayOffset { get; set; }

    public TimeOnly? ArrivalTime { get; set; }

    public TimeOnly? DepartureTime { get; set; }

    public virtual Route Route { get; set; } = null!;

    public virtual Station Station { get; set; } = null!;
}
