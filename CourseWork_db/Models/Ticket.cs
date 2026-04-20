using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public int PassengerId { get; set; }

    public int SeatId { get; set; }

    public int FromStationId { get; set; }

    public int ToStationId { get; set; }

    public float Price { get; set; }

    public virtual Station FromStation { get; set; } = null!;

    public virtual User Passenger { get; set; } = null!;

    public virtual Seat Seat { get; set; } = null!;

    public virtual Station ToStation { get; set; } = null!;

    public virtual Trip Trip { get; set; } = null!;
}
