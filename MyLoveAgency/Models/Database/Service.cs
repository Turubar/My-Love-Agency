using System;
using System.Collections.Generic;

namespace MyLoveAgency.Models.Database;

public partial class Service
{
    public int Id { get; set; }

    public string NameEn { get; set; } = null!;

    public string NameUa { get; set; } = null!;

    public string? DescriptionEn { get; set; }

    public string? DescriptionUa { get; set; }

    public string PriceZloty { get; set; } = null!;

    public int IdType { get; set; }

    public virtual TypeService IdTypeNavigation { get; set; } = null!;

    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();

    public virtual ICollection<StorageImageService> StorageImageServices { get; set; } = new List<StorageImageService>();
}
