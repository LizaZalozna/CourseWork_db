using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class CarTypeAllowedCharacteristic
{
    public int Id { get; set; }

    public int CarTypeId { get; set; }

    public int SeatCharacteristicId { get; set; }

    public virtual CarType CarType { get; set; } = null!;

    public virtual SeatCharacteristic SeatCharacteristic { get; set; } = null!;
}
