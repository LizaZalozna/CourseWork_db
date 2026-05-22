using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class Trip
{
    public int Id { get; set; }

    public int RouteId { get; set; }

    public int TrainId { get; set; }

    public DateOnly DepartureDate { get; set; }

    public DateOnly ArrivalDate { get; set; }

    public virtual Route Route { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual Train Train { get; set; } = null!;
}
