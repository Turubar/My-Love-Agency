using System;
using System.Collections.Generic;

namespace MyLoveAgency.Models.Database;

public partial class StorageImageGallery
{
    public int Id { get; set; }

    public string Path { get; set; } = null!;

    public int Number { get; set; }
}
