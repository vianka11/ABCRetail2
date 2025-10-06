using ABCRetail2.Models;
using ABCRetail2.Services;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly TableStorageService _tables;

    public HomeController(TableStorageService tables)
    {
        _tables = tables;
    }

    // GET: Home/Index
    public IActionResult Index()
    {
        var products = _tables.Query<ProductEntity>(_tables.Products, "PartitionKey eq 'PRODUCT'").ToList();

        return View(products);
    }
}