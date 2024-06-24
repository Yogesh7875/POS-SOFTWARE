using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace POS_Software.Models
{
    public class SaleViewModel
    {
        
        public string CustomerName { get; set; }
       
        public string CustomerPhone { get; set; }
        [Required(ErrorMessage = "Discount is required.")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
        public decimal Discount { get; set; }

        [Required(ErrorMessage = "Amount Paid is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount Paid must be a positive number.")]
        public decimal AmountPaid { get; set; }
        [Required(ErrorMessage = "Payment Mode is required.")]
        public string PaymentMode { get; set; }
        public int SaleId { get; set; }
        public System.DateTime SalesDate { get; set; }

        [Required(ErrorMessage = "At least one sale detail is required.")]
        public List<SalesDetailViewModel> SalesDetails { get; set; }
    }
}