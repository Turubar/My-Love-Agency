using System;
using System.Collections.Generic;

namespace MyLoveAgency.Models.Database;

public partial class TypeService
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public string NameUa { get; set; } = null!;

    public string NamePl { get; set; } = null!;

    public string? DescriptionEn { get; set; }

    public string? DescriptionUa { get; set; }

    public string? DescriptionPl { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
