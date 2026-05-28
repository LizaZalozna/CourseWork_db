using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class SeatCharacteristicType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<SeatCharacteristic> SeatCharacteristics { get; set; } = new List<SeatCharacteristic>();
}
