using System;
using System.Collections.Generic;

namespace CourseWork_db.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Login { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string? Email { get; set; }

    public string Password { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
