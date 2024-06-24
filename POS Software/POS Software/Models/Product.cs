using System;
using System.Collections.Generic;

namespace POS_Software.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Supplier { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int PurchasePrice { get; set; }

    public int Stock { get; set; }

    public int PerPack { get; set; }

    public decimal TotalPurchasePrice { get; set; }

    public decimal TotalSalePrice { get; set; }

    public bool Saleable { get; set; }
}
