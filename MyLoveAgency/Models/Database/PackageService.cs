using System;
using System.Collections.Generic;

namespace MyLoveAgency.Models.Database;

public partial class PackageService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string PriceZloty { get; set; } = null!;

    public string? DurationEn { get; set; }

    public string? DurationUa { get; set; }

    public string? DescriptionEn { get; set; }

    public string? DescriptionUa { get; set; }

    public int IdService { get; set; }

    public virtual Service IdServiceNavigation { get; set; } = null!;
}
