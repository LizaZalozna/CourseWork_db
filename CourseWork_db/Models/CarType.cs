using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class CarType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
