using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class SeatCharacteristic
{
    public int Id { get; set; }

    public int CharacteristicTypeId { get; set; }

    public string Value { get; set; } = null!;

    public virtual ICollection<CarTypeAllowedCharacteristic> CarTypeAllowedCharacteristics { get; set; } = new List<CarTypeAllowedCharacteristic>();

    public virtual SeatCharacteristicType CharacteristicType { get; set; } = null!;

    public virtual ICollection<SeatCharacteristicMap> SeatCharacteristicMaps { get; set; } = new List<SeatCharacteristicMap>();
}
