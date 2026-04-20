using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class RouteSegment
{
    public int Id { get; set; }

    public int RouteId { get; set; }

    public int FromStationId { get; set; }

    public int ToStationId { get; set; }

    public float Distance { get; set; }

    public virtual Station FromStation { get; set; } = null!;

    public virtual Route Route { get; set; } = null!;

    public virtual Station ToStation { get; set; } = null!;
}
