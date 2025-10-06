using ABCRetail2.Models;
using ABCRetail2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetail2.Controllers
{
    public class OrdersController : Controller
    {
        private readonly TableStorageService _tables;
        private readonly QueueService _queue;

        public OrdersController(TableStorageService tables, QueueService queue)
        {
            _tables = tables;
            _queue = queue;
        }

        // GET: Orders
        public IActionResult Index()
        {
            var orders = _tables.Query<OrderEntity>(_tables.Orders, "PartitionKey eq 'ORDER'")
                                .OrderByDescending(o => o.CreatedUtc)
                                .ToList();

            var customers = _tables.Query<CustomerEntity>(_tables.Customers, "PartitionKey eq 'CUSTOMER'")
                                   .ToDictionary(c => c.RowKey);

            var products = _tables.Query<ProductEntity>(_tables.Products, "PartitionKey eq 'PRODUCT'")
                                  .ToDictionary(p => p.RowKey);

            var viewModels = orders.Select(o => new OrderViewModel
            {
                RowKey = o.RowKey,
                CustomerName = customers.ContainsKey(o.CustomerId) ? $"{customers[o.CustomerId].FirstName} {customers[o.CustomerId].LastName}" : "Unknown",
                ProductName = products.ContainsKey(o.ProductId) ? products[o.ProductId].Name : "Unknown",
                Quantity = o.Quantity,
                UnitPrice = o.UnitPrice,
                Total = o.Total,
                Status = o.Status,
                CreatedUtc = o.CreatedUtc
            }).ToList();

            return View(viewModels);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewBag.Customers = _tables.Query<CustomerEntity>(_tables.Customers, "PartitionKey eq 'CUSTOMER'").ToList();
            ViewBag.Products = _tables.Query<ProductEntity>(_tables.Products, "PartitionKey eq 'PRODUCT'").ToList();
            return View(new OrderEntity { Quantity = 1 });
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderEntity model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Customers = _tables.Query<CustomerEntity>(_tables.Customers, "PartitionKey eq 'CUSTOMER'").ToList();
                ViewBag.Products = _tables.Query<ProductEntity>(_tables.Products, "PartitionKey eq 'PRODUCT'").ToList();
                return View(model);
            }

            var product = await _tables.GetAsync<ProductEntity>(_tables.Products, "PRODUCT", model.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("", "Product not found.");
                return View(model);
            }

            model.PartitionKey = "ORDER";
            model.RowKey = TableStorageService.NewId();
            model.UnitPrice = product.Price;
            model.Total = model.UnitPrice * (model.Quantity > 0 ? model.Quantity : 1);
            model.Quantity = model.Quantity > 0 ? model.Quantity : 1;
            model.Status = "Pending";
            model.CreatedUtc = System.DateTime.UtcNow;

            // Write to Orders table
            await _tables.UpsertAsync(_tables.Orders, model);

            // Enqueue a message for processing
            await _queue.EnqueueOrderEventAsync(new
            {
                type = "OrderCreated",
                orderId = model.RowKey,
                customerId = model.CustomerId,
                productId = model.ProductId,
                quantity = model.Quantity,
                message = $"Processing order {model.RowKey}"
            });

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var order = await _tables.GetAsync<OrderEntity>(_tables.Orders, "ORDER", id);
            if (order == null) return NotFound();

            var customer = await _tables.GetAsync<CustomerEntity>(_tables.Customers, "CUSTOMER", order.CustomerId);
            var product = await _tables.GetAsync<ProductEntity>(_tables.Products, "PRODUCT", order.ProductId);

            var viewModel = new OrderViewModel
            {
                RowKey = order.RowKey,
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                ProductName = product?.Name ?? "Unknown",
                Quantity = order.Quantity,
                UnitPrice = order.UnitPrice,
                Total = order.Total,
                Status = order.Status,
                CreatedUtc = order.CreatedUtc
            };

            return View(viewModel);
        }

        // GET: Orders/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var order = await _tables.GetAsync<OrderEntity>(_tables.Orders, "ORDER", id);
            if (order == null) return NotFound();

            ViewBag.Customers = _tables.Query<CustomerEntity>(_tables.Customers, "PartitionKey eq 'CUSTOMER'").ToList();
            ViewBag.Products = _tables.Query<ProductEntity>(_tables.Products, "PartitionKey eq 'PRODUCT'").ToList();

            return View(order);
        }

        // POST: Orders/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, OrderEntity model)
        {
            if (!ModelState.IsValid) return View(model);

            var order = await _tables.GetAsync<OrderEntity>(_tables.Orders, "ORDER", id);
            if (order == null) return NotFound();

            order.CustomerId = model.CustomerId;
            order.ProductId = model.ProductId;
            order.Quantity = model.Quantity;
            order.UnitPrice = model.UnitPrice;
            order.Total = order.Quantity * order.UnitPrice;
            order.Status = model.Status;

            await _tables.UpsertAsync(_tables.Orders, order);

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var order = await _tables.GetAsync<OrderEntity>(_tables.Orders, "ORDER", id);
            if (order == null) return NotFound();

            var customer = await _tables.GetAsync<CustomerEntity>(_tables.Customers, "CUSTOMER", order.CustomerId);
            var product = await _tables.GetAsync<ProductEntity>(_tables.Products, "PRODUCT", order.ProductId);

            var viewModel = new OrderViewModel
            {
                RowKey = order.RowKey,
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                ProductName = product?.Name ?? "Unknown",
                Quantity = order.Quantity,
                UnitPrice = order.UnitPrice,
                Total = order.Total,
                Status = order.Status,
                CreatedUtc = order.CreatedUtc
            };

            return View(viewModel);
        }

        // POST: Orders/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return BadRequest();

            await _tables.DeleteAsync(_tables.Orders, "ORDER", rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
