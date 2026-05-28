using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class CarType
{
    public int Id { get; set; }

    public int CarTypeNameId { get; set; }

    public int ModernizationStageId { get; set; }

    public float PricePerKm { get; set; }

    public float ServicePrice { get; set; }

    public virtual ICollection<CarTypeAllowedCharacteristic> CarTypeAllowedCharacteristics { get; set; } = new List<CarTypeAllowedCharacteristic>();

    public virtual CarTypeName CarTypeName { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    public virtual ModernizationStage ModernizationStage { get; set; } = null!;
}
