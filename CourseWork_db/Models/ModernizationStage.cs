using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class ModernizationStage
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CarType> CarTypes { get; set; } = new List<CarType>();
}
