using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using ABCRetail2.Models;
using System.Threading.Tasks;

namespace ABCRetailPt3.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT UserId, Role 
                FROM Users 
                WHERE Username = @Username AND PasswordHash = HASHBYTES('SHA2_256', @Password)", conn);

            cmd.Parameters.AddWithValue("@Username", model.Username);
            cmd.Parameters.AddWithValue("@Password", model.Password);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var userId = reader.GetInt32(0);
                var role = reader.GetString(1);

                HttpContext.Session.SetString("UserId", userId.ToString());
                HttpContext.Session.SetString("Role", role);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}