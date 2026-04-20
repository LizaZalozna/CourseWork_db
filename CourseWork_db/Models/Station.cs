using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class Station
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;

    public virtual ICollection<RouteSegment> RouteSegmentFromStations { get; set; } = new List<RouteSegment>();

    public virtual ICollection<RouteSegment> RouteSegmentToStations { get; set; } = new List<RouteSegment>();

    public virtual ICollection<RouteStation> RouteStations { get; set; } = new List<RouteStation>();

    public virtual ICollection<Ticket> TicketFromStations { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketToStations { get; set; } = new List<Ticket>();
}
