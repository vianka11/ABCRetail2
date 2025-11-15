using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ABCRetail2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;

namespace ABCRetailPt3.Controllers
{
    public class CartController : Controller
    {
        private readonly IConfiguration _config;

        public CartController(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var cart = new CartModel { CustomerId = int.Parse(userId) };

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT ci.CartItemId, ci.ProductId, p.Name, p.Price, ci.Quantity
                FROM CartItems ci
                JOIN Products p ON ci.ProductId = p.ProductId
                JOIN Cart c ON ci.CartId = c.CartId
                WHERE c.CustomerId = @CustomerId", conn);

            cmd.Parameters.AddWithValue("@CustomerId", cart.CustomerId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cart.Items.Add(new CartItemModel
                {
                    CartItemId = reader.GetInt32(0),
                    ProductId = reader.GetInt32(1),
                    ProductName = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    Quantity = reader.GetInt32(4)
                });
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            var userId = HttpContext.Session.GetString("UserId");

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            // Get or create cart
            var getCartCmd = new SqlCommand("SELECT CartId FROM Cart WHERE CustomerId = @CustomerId", conn);
            getCartCmd.Parameters.AddWithValue("@CustomerId", userId);
            var cartId = await getCartCmd.ExecuteScalarAsync();

            if (cartId == null)
            {
                var createCartCmd = new SqlCommand("INSERT INTO Cart (CustomerId) OUTPUT INSERTED.CartId VALUES (@CustomerId)", conn);
                createCartCmd.Parameters.AddWithValue("@CustomerId", userId);
                cartId = await createCartCmd.ExecuteScalarAsync();
            }

            // Add item
            var addItemCmd = new SqlCommand(@"
                INSERT INTO CartItems (CartId, ProductId, Quantity)
                VALUES (@CartId, @ProductId, @Quantity)", conn);

            addItemCmd.Parameters.AddWithValue("@CartId", cartId);
            addItemCmd.Parameters.AddWithValue("@ProductId", productId);
            addItemCmd.Parameters.AddWithValue("@Quantity", quantity);

            await addItemCmd.ExecuteNonQueryAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetString("UserId");

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var getCartCmd = new SqlCommand("SELECT CartId FROM Cart WHERE CustomerId = @CustomerId", conn);
            getCartCmd.Parameters.AddWithValue("@CustomerId", userId);
            var cartId = await getCartCmd.ExecuteScalarAsync();

            if (cartId != null)
            {
                var placeOrderCmd = new SqlCommand(@"
                    INSERT INTO Orders (CustomerId, CartId, Status)
                    VALUES (@CustomerId, @CartId, 'Pending')", conn);

                placeOrderCmd.Parameters.AddWithValue("@CustomerId", userId);
                placeOrderCmd.Parameters.AddWithValue("@CartId", cartId);

                await placeOrderCmd.ExecuteNonQueryAsync();
            }

            return RedirectToAction("Index", "Order");
        }
    }
}