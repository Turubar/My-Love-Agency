using System;
using System.Collections.Generic;

namespace MyLoveAgency.Models.Database;

public partial class Contact
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Communication { get; set; }

    public string? Message { get; set; }

    public DateTime Date { get; set; }

    public int IdService { get; set; }

    public int? IdPackage { get; set; }
}
