using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace ABCRetailFunctions.Services
{
    public class BlobService
    {
        private readonly BlobContainerClient _container;

        public BlobService(string connectionString, string containerName)
        {
            _container = new BlobServiceClient(connectionString)
                .GetBlobContainerClient(containerName);
            _container.CreateIfNotExists();
        }

        public async Task<string> UploadAsync(string fileName, byte[] data)
        {
            var blobClient = _container.GetBlobClient(fileName);
            await using var ms = new MemoryStream(data);
            await blobClient.UploadAsync(ms, overwrite: true);
            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var blobClient = _container.GetBlobClient(fileName);
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);
            return blobClient.Uri.ToString();
        }
    }
}
