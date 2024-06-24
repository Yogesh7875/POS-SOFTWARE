using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Software.Models
{
    public class PurchaseViewModel
    {
        public string SupplierName { get; set; }
        public string SupplierAddress { get; set; }
        public string PaymentMode { get; set; }
        public decimal Discount { get; set; }
        public decimal AmountPaid { get; set; }
        public int PurchaseId { get; set; }
        public System.DateTime PurchaseDate { get; set; }
        public List<PurchaseDetailViewModel> PurchaseDetails { get; set; }
    }
}