using System;
using System.Collections.Generic;

namespace MyLoveAgency.Models.Database;

public partial class Localization
{
    public string Name { get; set; } = string.Empty;

    public string? En { get; set; }

    public string? Ua { get; set; }

    public string? Pl { get; set; }
}
