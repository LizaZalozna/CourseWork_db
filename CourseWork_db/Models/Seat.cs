using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class Seat
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public int SeatNumber { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual ICollection<SeatCharacteristicMap> SeatCharacteristicMaps { get; set; } = new List<SeatCharacteristicMap>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
