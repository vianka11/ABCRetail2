using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;

namespace ABCRetailFunctions.Services
{
    public class AzureFileShareService
    {
        private readonly ShareClient _shareClient;
        private readonly string _directoryName;

        public AzureFileShareService(string connectionString, string shareName, string directory)
        {
            _shareClient = new ShareClient(connectionString, shareName);
            _shareClient.CreateIfNotExists();

            _directoryName = directory;
            var dirClient = _shareClient.GetDirectoryClient(_directoryName);
            dirClient.CreateIfNotExists();
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            var safeName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var dirClient = _shareClient.GetDirectoryClient(_directoryName);
            var fileClient = dirClient.GetFileClient(safeName);

            await fileClient.CreateAsync(file.Length);
            await using var stream = file.OpenReadStream();
            await fileClient.UploadRangeAsync(new Azure.HttpRange(0, file.Length), stream);

            return fileClient.Uri.ToString();
        }
    }
}
