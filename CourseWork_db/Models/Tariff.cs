using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class Tariff
{
    public int Id { get; set; }

    public int CarTypeId { get; set; }

    public float PricePerKm { get; set; }

    public virtual CarType CarType { get; set; } = null!;
}
