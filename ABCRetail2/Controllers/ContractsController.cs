using ABCRetail2.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail2.Controllers
{
    public class ContractsController : Controller
    {
        // File uploads to Azure File Share
        private readonly AzureFileShareService _files;

        public ContractsController(AzureFileShareService files) => _files = files;

        // Contracts section
        public IActionResult Index() => View();

        // File upload requests
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // Check if no file was selected
            if (file == null)
            {
                TempData["Msg"] = "Please select a file.";
                return RedirectToAction(nameof(Index));
            }

            // Upload file using AzureFileShareService
            var url = await _files.UploadAsync(file);

            TempData["Msg"] = $"Uploaded. URL (internal): {url}";

            return RedirectToAction(nameof(Index));
        }
    }
}