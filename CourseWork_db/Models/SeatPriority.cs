using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class SeatPriority
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<SeatPriorityPerTrip> SeatPriorityPerTrips { get; set; } = new List<SeatPriorityPerTrip>();
}
