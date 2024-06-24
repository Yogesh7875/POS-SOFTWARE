using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json;
using POS_Software.Models;

public class SalesController : Controller
{
    private readonly POSDBEntities1 _context;

    public SalesController()
    {
        _context = new POSDBEntities1();
    }

    // GET: Sales
    public ActionResult Index()
    {
        var sales = _context.Sales.OrderByDescending(s => s.SaleId).ToList();
        return View(sales);
    }

    // GET: Sales/Create
    [HttpGet]
    public ActionResult Create()
    {
        PopulateViewBags();
        return View(new SaleViewModel { SalesDetails = new List<SalesDetailViewModel>() });
    }

    // POST: Sales/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(SaleViewModel model)
    {
        if (ModelState.IsValid)
        {
            var productsToUpdate = new List<Product>();

            // Check stock availability
            foreach (var detail in model.SalesDetails)
            {
                var product = _context.Products.SingleOrDefault(p => p.ProductId == detail.ProductId);
                if (product == null || product.ProductQuantity < detail.Quantity)
                {
                    ModelState.AddModelError("", $"Not enough stock for product {product?.ProductName ?? "Unknown"}");
                    PopulateViewBags();
                    return View(model);
                }

                product.ProductQuantity -= detail.Quantity;
                productsToUpdate.Add(product);
            }

            // Create and save the sale
            var sale = new Sale
            {
                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                Discount = model.Discount,
                AmountPaid = model.AmountPaid,
                PaymentMode = model.PaymentMode,
                SalesDate = DateTime.Today,
                SalesDetails = model.SalesDetails.Select(d => new SalesDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalPrice = d.TotalPrice
                }).ToList()
            };

            _context.Sales.Add(sale);
            _context.SaveChanges();

            // Update product stocks
            foreach (var product in productsToUpdate)
            {
                _context.Entry(product).State = EntityState.Modified;
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        PopulateViewBags();
        return View(model ?? new SaleViewModel { SalesDetails = new List<SalesDetailViewModel>() });
    }

    public ActionResult Edit(int id)
    {
        var sale = _context.Sales
            .Include(s => s.SalesDetails)
            .SingleOrDefault(s => s.SaleId == id);

        var model = new SaleViewModel
        {
            SaleId = sale.SaleId,
            CustomerName = sale.CustomerName,
            CustomerPhone = sale.CustomerPhone,
            PaymentMode = sale.PaymentMode,
            SalesDetails = sale.SalesDetails.Select(sd => new SalesDetailViewModel
            {
                ProductId = sd.ProductId,
                UnitPrice = sd.UnitPrice,
                Quantity = sd.Quantity,
                TotalPrice = sd.TotalPrice,
                StockQuantity = _context.Products.SingleOrDefault(p => p.ProductId == sd.ProductId).ProductQuantity
            }).ToList()
        };

        ViewBag.Products = _context.Products
            .Select(p => new SelectListItem
            {
                Value = p.ProductId.ToString(),
                Text = p.ProductName
            })
            .ToList();

        ViewBag.ProductData = _context.Products
            .Select(p => new
            {
                p.ProductId,
                p.ProductName,
                p.ProductPrice,
                p.ProductQuantity
            })
            .ToList();

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(SaleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // If model state is not valid, return to the same view with validation errors
            ViewBag.Products = _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.ProductName
                })
                .ToList();

            ViewBag.ProductData = _context.Products
                .Select(p => new
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    StockQuantity = p.ProductQuantity
                })
                .ToList();

            return View(model);
        }

        // Update the sale in the database
        var saleInDb = _context.Sales
            .Include(s => s.SalesDetails) // Ensure SalesDetails are included
            .Single(s => s.SaleId == model.SaleId);

        saleInDb.CustomerName = model.CustomerName;
        saleInDb.PaymentMode = model.PaymentMode;
        saleInDb.CustomerPhone = model.CustomerPhone;

        // Update other properties as needed

        // Update existing sales details or add new ones
        foreach (var detail in model.SalesDetails)
        {
            var existingDetail = saleInDb.SalesDetails.FirstOrDefault(sd => sd.ProductId == detail.ProductId);

            if (existingDetail != null)
            {
                // Product already exists, overwrite existing detail
                existingDetail.UnitPrice = detail.UnitPrice;
                existingDetail.Quantity = detail.Quantity;
                existingDetail.TotalPrice = detail.TotalPrice;
                // Update other properties as needed
            }
            else
            {
                // Product doesn't exist, add new detail
                saleInDb.SalesDetails.Add(new SalesDetail
                {
                    ProductId = detail.ProductId,
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                    TotalPrice = detail.TotalPrice,
                    // Add other properties as needed
                });
            }

        }

        // Save changes
        _context.SaveChanges();

        // Redirect to a success page or another action
        return RedirectToAction("Index", "Sales"); // Replace with your desired action and controller
    }






    // GET: Sales/BillPrint/5
    public ActionResult BillPrint(int id)
    {
        var sale = _context.Sales
            .Include(s => s.SalesDetails.Select(sd => sd.Product))
            .FirstOrDefault(s => s.SaleId == id);

        if (sale == null)
        {
            return HttpNotFound();
        }

        var viewModel = new SaleViewModel
        {
            SaleId = sale.SaleId,
            CustomerName = sale.CustomerName,
            PaymentMode = sale.PaymentMode,
            Discount = sale.Discount,
            AmountPaid = sale.AmountPaid,
            SalesDate = sale.SalesDate,
            CustomerPhone = sale.CustomerPhone,
            SalesDetails = sale.SalesDetails.Select(sd => new SalesDetailViewModel
            {
                ProductId = sd.ProductId,
                ProductName = sd.Product.ProductName,
                UnitPrice = sd.UnitPrice,
                Quantity = sd.Quantity,
                TotalPrice = sd.TotalPrice
            }).ToList()
        };

        return View(viewModel);
    }

    // Dispose
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
        base.Dispose(disposing);
    }

    // Populate ViewBags
    private void PopulateViewBags()
    {
        var customers = _context.Customers.ToList();
        ViewBag.CustomerId = new SelectList(customers, "CustomerName", "CustomerName");

        var products = _context.Products.Select(p => new
        {
            p.ProductId,
            p.ProductName,
            p.ProductPrice,
            p.ProductQuantity
        }).ToList();
        ViewBag.Products = new SelectList(products, "ProductId", "ProductName");
        ViewBag.ProductData = products;
    }
}