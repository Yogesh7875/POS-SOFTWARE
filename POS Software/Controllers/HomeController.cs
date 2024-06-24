using POS_Software.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POS_Software.Controllers
{

    public class HomeController : Controller
    {
        POSDBEntities1 entities1 = new POSDBEntities1();
        private const string ValidUsername = "Factory";
        private const string ValidPassword = "1234";
        public ActionResult Index()
        {
            //customer
            var Customer = entities1.Customers.Count();
            if (Customer == null)
            {
                Customer = 0;
            }
            var Product = entities1.Products.Count();
            if (Product == null)
            {
                Product = 0;
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var totalPaidAmounts = entities1.Sales
                .Where(s => s.SalesDate >= today && s.SalesDate < tomorrow && s.AmountPaid > 0)
                .Sum(s => (decimal?)s.AmountPaid) ?? 0;



            //suppliers
            var suppliers = entities1.Suppliers.Count();
            if (suppliers == null)
            {
                suppliers = 0;
            }

            //Purchases
            var purchases = entities1.Purchases
                .Where(p => p.PurchaseDate >= today && p.PurchaseDate < tomorrow && p.AmountPaid > 0)
                .Sum(p => (decimal?)p.AmountPaid) ?? 0;

            // profit
            var profit = totalPaidAmounts - purchases;
            if (profit == null)
            {
                profit = 0;
            }
            var sal = entities1.Sales.Count();
            if (sal == null)
            {
                sal = 0;
            }
            var purch = entities1.Purchases.Count();
            if (purch == null)
            {
                purch = 0;
            }
            var stock = entities1.Products.Sum(p => p.ProductQuantity);
            if (stock == null)
            {
                stock = 0;
            }
            DashData data = new DashData()
            {
                TotalCustomer = Customer,
                TotalProducts = Product,
                TotalSales = (int)totalPaidAmounts,
                TotalSuppliers = suppliers,
                TotalPurchases = (int)purchases,
                TotalProfit = (int)profit,
                TotalSaleOrders = sal,
                TotalPurchaseOrders = purch,
                TotalStock = stock
            };
          

            return View(data);
        }

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {

            if (username == "Factory" && password == "1234")
            {
                // Authentication successful
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Authentication failed
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}