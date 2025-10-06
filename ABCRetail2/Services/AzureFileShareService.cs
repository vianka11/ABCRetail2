using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ABCRetail2.Services
{
    public class AzureFileShareService
    {
        private readonly ShareClient _shareClient;
        private readonly string _directoryName;

        // Constructor with DI for options
        public AzureFileShareService(IOptions<AzureStorageOptions> options)
        {
            var o = options.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(o.FileShare?.ShareName))
                throw new ArgumentException("FileShare.ShareName must be configured in AzureStorageOptions.");

            _directoryName = string.IsNullOrWhiteSpace(o.FileShare.Directory) ? "contracts" : o.FileShare.Directory;

            // Initialize ShareClient
            _shareClient = new ShareClient(o.ConnectionString, o.FileShare.ShareName);

            try
            {
                _shareClient.CreateIfNotExists();
                var dirClient = _shareClient.GetDirectoryClient(_directoryName);
                dirClient.CreateIfNotExists();
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"[AzureFileShareService] Failed to create share/directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads a file to Azure File Share
        /// </summary>
        /// <param name="file">IFormFile from HTTP request</param>
        /// <returns>Uri of uploaded file</returns>
        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("File is null or empty.");

            // Generate a safe unique file name
            var safeName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";

            var dirClient = _shareClient.GetDirectoryClient(_directoryName);
            var fileClient = dirClient.GetFileClient(safeName);

            try
            {
                // Create the file on Azure File Share
                await fileClient.CreateAsync(file.Length);

                // Upload content in a single range
                await using var stream = file.OpenReadStream();
                await fileClient.UploadRangeAsync(new HttpRange(0, file.Length), stream);

                return fileClient.Uri.ToString();
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"[AzureFileShareService] Failed to upload file: {ex.Message}");
                throw;
            }
        }
    }
}
