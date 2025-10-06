using Azure.Data.Tables;

namespace ABCRetailFunctions.Services
{
    public class TableStorageService
    {
        private readonly TableServiceClient _tableService;

        public TableStorageService(string connectionString)
        {
            _tableService = new TableServiceClient(connectionString);
        }

        public TableClient GetTableClient(string tableName)
        {
            var client = _tableService.GetTableClient(tableName);
            client.CreateIfNotExists();
            return client;
        }
    }
}
