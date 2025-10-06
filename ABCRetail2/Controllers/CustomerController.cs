using ABCRetail2.Models;
using ABCRetail2.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail2.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableStorageService _tables;

        public CustomersController(TableStorageService tables) => _tables = tables;

        // GET: Customers
        public IActionResult Index()
        {
            var list = _tables.Query<CustomerEntity>(_tables.Customers, "PartitionKey eq 'CUSTOMER'").ToList();
            return View(list);
        }

        // GET: Customers/Create
        public IActionResult Create() => View();

        // POST: Customers/Create
        [HttpPost]
        public async Task<IActionResult> Create(CustomerEntity model)
        {
            if (!ModelState.IsValid) return View(model); // Validation check

            // Set required Azure Table Storage keys
            model.PartitionKey = "CUSTOMER";
            model.RowKey = TableStorageService.NewId();

            await _tables.UpsertAsync(_tables.Customers, model);

            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var entity = await _tables.GetAsync<CustomerEntity>("CUSTOMER", id, _tables.Customers);
            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: Customers/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(string id, CustomerEntity model)
        {
            if (!ModelState.IsValid) return View(model); // Validation check

            // Ensure correct keys are set before saving
            model.PartitionKey = "CUSTOMER";
            model.RowKey = id;

            await _tables.UpsertAsync(_tables.Customers, model);

            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _tables.GetAsync<CustomerEntity>("CUSTOMER", id, _tables.Customers);
            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: Customers/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _tables.DeleteAsync(_tables.Customers, "CUSTOMER", id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var entity = await _tables.GetAsync<CustomerEntity>("CUSTOMER", id, _tables.Customers);
            if (entity == null) return NotFound();
            return View(entity);
        }
    }

    public static class TableStorageExtensions
    {
        // Retrieves entity from Azure Table Storage 
        public static Task<T?> GetAsync<T>(this TableStorageService svc, string pk, string rk, Azure.Data.Tables.TableClient client)
            where T : class, Azure.Data.Tables.ITableEntity, new()
            => svc.GetAsync<T>(client, pk, rk);
    }
}
