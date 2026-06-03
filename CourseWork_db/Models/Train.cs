using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class Train
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
