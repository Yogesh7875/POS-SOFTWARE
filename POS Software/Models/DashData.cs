using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Software.Models
{
    public class DashData
    {
        public int TotalCustomer { get; set; }
        public int TotalPurchases { get; set; }
        public int TotalSales { get; set; }
        public int TotalProfit { get; set; }
        public int TotalSaleOrders { get; set; }
        public int TotalPurchaseOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalStock { get; set; }

    }
}