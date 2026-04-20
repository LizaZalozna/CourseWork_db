using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class Route
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<RouteSegment> RouteSegments { get; set; } = new List<RouteSegment>();

    public virtual ICollection<RouteStation> RouteStations { get; set; } = new List<RouteStation>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
