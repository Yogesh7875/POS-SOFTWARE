using System;
using System.Collections.Generic;

namespace POS_Software.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int Balance { get; set; }

    public string? Contact { get; set; }
}
