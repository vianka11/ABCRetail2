using ABCRetail2.Models;
using ABCRetail2.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetail1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly TableStorageService _tables;
        private readonly BlobService _blobs;

        public ProductsController(TableStorageService tables, BlobService blobs)
        {
            _tables = tables;
            _blobs = blobs;
        }

        // GET: /Products
        public IActionResult Index(string searchTerm)
        {
            var products = _tables.Query<ProductEntity>(_tables.Products, "PartitionKey eq 'PRODUCT'");

            // Apply search filter 
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
                ViewBag.SearchTerm = searchTerm;
            }

            var list = products.ToList();

            if (!list.Any())
                ViewBag.Message = "No products found.";

            return View(list);
        }

        // GET: /Products/Create
        public IActionResult Create() => View();

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEntity model, IFormFile? image)
        {
            // Validate price
            var priceRaw = Request.Form["Price"].ToString().Replace("R", "").Replace("r", "").Trim().Replace(',', '.');
            if (!decimal.TryParse(priceRaw, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedPrice))
            {
                ModelState.AddModelError("Price", "Invalid price format. Enter numbers only, e.g., 100 or 100.50");
                return View(model);
            }
            model.Price = parsedPrice;

            if (!ModelState.IsValid) return View(model);

            model.PartitionKey = "PRODUCT";
            model.RowKey = TableStorageService.NewId();

            if (image != null && image.Length > 0)
            {
                model.ImageUrl = await _blobs.UploadAsync(image);
            }

            await _tables.UpsertAsync(_tables.Products, model);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var entity = await _tables.GetAsync<ProductEntity>(_tables.Products, "PRODUCT", id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: /Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProductEntity model, IFormFile? image)
        {
            var entity = await _tables.GetAsync<ProductEntity>(_tables.Products, "PRODUCT", id);
            if (entity == null) return NotFound();

            // Validate price
            var priceRaw = Request.Form["Price"].ToString().Replace("R", "").Replace("r", "").Trim().Replace(',', '.');
            if (!decimal.TryParse(priceRaw, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedPrice))
            {
                ModelState.AddModelError("Price", "Invalid price format. Enter numbers only, e.g., 100 or 100.50");
                return View(model);
            }
            model.Price = parsedPrice;

            if (!ModelState.IsValid) return View(model);

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Price = model.Price;
            entity.StockQuantity = model.StockQuantity;

            if (image != null && image.Length > 0)
            {
                entity.ImageUrl = await _blobs.UploadAsync(image);
            }

            await _tables.UpsertAsync(_tables.Products, entity);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var entity = await _tables.GetAsync<ProductEntity>(_tables.Products, "PRODUCT", id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        // GET: /Products/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _tables.GetAsync<ProductEntity>(_tables.Products, "PRODUCT", id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: /Products/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _tables.DeleteAsync(_tables.Products, "PRODUCT", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
