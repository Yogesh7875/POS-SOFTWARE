using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Software.Models
{
    public class SalesDetailViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ProductName { get; set; }
        public int StockQuantity { get; set; }
    }
}