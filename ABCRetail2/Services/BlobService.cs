using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ABCRetail2.Services
{
    public class BlobService
    {
        private readonly BlobContainerClient _container;

        /// <summary>
        /// Constructor - initializes BlobServiceClient and container
        /// </summary>
        public BlobService(IOptions<AzureStorageOptions> options)
        {
            var o = options.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(o.Blob?.Container))
                throw new ArgumentException("Blob container name must be configured in AzureStorageOptions.");

            var blobServiceClient = new BlobServiceClient(o.ConnectionString);

            _container = blobServiceClient.GetBlobContainerClient(o.Blob.Container);

            // Ensure the container exists and is public for reading
            _container.CreateIfNotExists(PublicAccessType.Blob);
        }

        /// <summary>
        /// Uploads an IFormFile to Blob Storage
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="fileName">Optional file name; generates GUID if null</param>
        /// <returns>URL of uploaded blob</returns>
        public async Task<string> UploadAsync(IFormFile file, string? fileName = null)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file provided.");

            // Generate unique filename if not provided
            fileName ??= $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";

            var blobClient = _container.GetBlobClient(fileName);

            try
            {
                await using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlobService] Error uploading file: {ex.Message}");
                throw;
            }

            return blobClient.Uri.ToString();
        }
    }
}
