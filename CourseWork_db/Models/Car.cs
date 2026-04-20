using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class Car
{
    public int Id { get; set; }

    public int SeatsCount { get; set; }

    public int TrainId { get; set; }

    public int CarTypeId { get; set; }

    public int CarNumber { get; set; }

    public virtual CarType CarType { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual Train Train { get; set; } = null!;
}
