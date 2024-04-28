using System;
using System.Collections.Generic;

namespace MyLoveAgency.Models.Database;

public partial class StorageImageService
{
    public int Id { get; set; }

    public string Path { get; set; } = null!;

    public int Number { get; set; }

    public int IdService { get; set; }

    public virtual Service IdServiceNavigation { get; set; } = null!;
}
