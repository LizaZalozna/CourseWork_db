using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class SeatPriorityPerTrip
{
    public int Id { get; set; }

    public int SeatId { get; set; }

    public int TripId { get; set; }

    public int SeatPriorityId { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual SeatPriority SeatPriority { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
