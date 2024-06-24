using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using POS_Software.Models;

public class PurchasesController : Controller
{
    private readonly POSDBEntities1 _context;

    public PurchasesController()
    {
        _context = new POSDBEntities1();
    }

    public ActionResult Index()
    {
        var purchases = _context.Purchases.ToList();
        return View(purchases);
    }



    [HttpGet]
    public ActionResult Create()
    {
        PopulateViewBags();
        return View(new PurchaseViewModel { PurchaseDetails = new List<PurchaseDetailViewModel>() });
    }

    // POST: Purchases/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(PurchaseViewModel model)
    {
        if (ModelState.IsValid)
        {
            var productsToUpdate = new List<Product>();

            // Check stock availability and update quantities
            foreach (var detail in model.PurchaseDetails)
            {
                var product = _context.Products.SingleOrDefault(p => p.ProductId == detail.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", $"Product with ID {detail.ProductId} not found");
                    PopulateViewBags();
                    return View(model);
                }

                product.ProductQuantity += detail.Quantity;
                productsToUpdate.Add(product);
            }

            // Create and save the purchase
            var purchase = new Purchase
            {
                SupplierName = model.SupplierName,
                SupplierAddress = model.SupplierAddress,
                Discount = model.Discount,
                AmountPaid = model.AmountPaid,
                PaymentMode = model.PaymentMode,
                PurchaseDate = DateTime.Today,
                PurchaseDetails = model.PurchaseDetails.Select(d => new PurchaseDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalPrice = d.TotalPrice
                }).ToList()
            };

            _context.Purchases.Add(purchase);
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
        return View(model ?? new PurchaseViewModel { PurchaseDetails = new List<PurchaseDetailViewModel>() });
    }

    // Helper method to populate ViewBag with necessary data
    private void PopulateViewBags()
    {
        ViewBag.SupplierId = new SelectList(_context.Suppliers, "SupplierName", "SupplierName");
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




    [HttpGet]
    public ActionResult Edit(int id)
    {
        var purchase = _context.Purchases
            .Include("PurchaseDetails")  // Using string-based Include
            .FirstOrDefault(p => p.PurchaseId == id);

        if (purchase == null)
        {
            return HttpNotFound();
        }

        var viewModel = new PurchaseViewModel
        {
            PurchaseId = purchase.PurchaseId,
            SupplierName = purchase.SupplierName,
            SupplierAddress = purchase.SupplierAddress,
            PaymentMode = purchase.PaymentMode,
            Discount = (decimal)purchase.Discount,
            AmountPaid = purchase.AmountPaid,
            PurchaseDetails = purchase.PurchaseDetails.Select(pd => new PurchaseDetailViewModel
            {
                ProductId = pd.ProductId,
                UnitPrice = pd.UnitPrice,
                Quantity = pd.Quantity,
                TotalPrice = pd.TotalPrice
            }).ToList()
        };

        ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName");
        ViewBag.SupplierId = new SelectList(_context.Suppliers, "SupplierName", "SupplierName");

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, PurchaseViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var purchase = _context.Purchases
                .Include("PurchaseDetails")  // Using string-based Include
                .FirstOrDefault(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return HttpNotFound();
            }

            purchase.SupplierName = viewModel.SupplierName;
            purchase.SupplierAddress = viewModel.SupplierAddress;
            purchase.PaymentMode = viewModel.PaymentMode;
            purchase.Discount = viewModel.Discount;
            purchase.AmountPaid = viewModel.AmountPaid;

            // Remove existing PurchaseDetails and add new ones
            _context.PurchaseDetails.RemoveRange(purchase.PurchaseDetails);
            purchase.PurchaseDetails = viewModel.PurchaseDetails.Select(d => new PurchaseDetail
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TotalPrice = d.TotalPrice
            }).ToList();

            _context.Entry(purchase).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName", viewModel.PurchaseDetails.FirstOrDefault()?.ProductId);
        ViewBag.SupplierId = new SelectList(_context.Suppliers, "SupplierName", "SupplierName", viewModel.SupplierName);

        return View(viewModel);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
        base.Dispose(disposing);
    }

    public ActionResult PurchaseBill(int id)
    {
        var Purchase = _context.Purchases
            .Include(s => s.PurchaseDetails.Select(sd => sd.Product))
            .FirstOrDefault(s => s.PurchaseId == id);

        if (Purchase == null)
        {
            return HttpNotFound();
        }

        var viewModel = new PurchaseViewModel
        {
            PurchaseId = Purchase.PurchaseId,
            SupplierName = Purchase.SupplierName,
            SupplierAddress = Purchase.SupplierAddress,
            PaymentMode = Purchase.PaymentMode,
            Discount = (decimal)Purchase.Discount,
            PurchaseDate = Purchase.PurchaseDate,
            AmountPaid = Purchase.AmountPaid,
            PurchaseDetails = Purchase.PurchaseDetails.Select(sd => new PurchaseDetailViewModel
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
}