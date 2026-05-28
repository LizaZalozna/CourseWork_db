using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class SeatCharacteristicMap
{
    public int Id { get; set; }

    public int SeatId { get; set; }

    public int SeatCharacteristicId { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual SeatCharacteristic SeatCharacteristic { get; set; } = null!;
}
