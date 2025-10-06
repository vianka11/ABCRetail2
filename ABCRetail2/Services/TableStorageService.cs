using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace ABCRetail2.Services
{
    public class TableStorageService
    {
        private readonly AzureStorageOptions _options;
        private readonly TableServiceClient _serviceClient;

        public TableStorageService(IOptions<AzureStorageOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));

            _serviceClient = new TableServiceClient(_options.ConnectionString);

            // Ensure the tables exist
            CreateTablesIfNotExists();
        }

        private void CreateTablesIfNotExists()
        {
            try
            {
                _serviceClient.CreateTableIfNotExists(_options.Tables.Customer);
                _serviceClient.CreateTableIfNotExists(_options.Tables.Product);
                _serviceClient.CreateTableIfNotExists(_options.Tables.Order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStorageService] Failed to ensure tables exist: {ex.Message}");
            }
        }

        public TableClient Customers => _serviceClient.GetTableClient(_options.Tables.Customer);
        public TableClient Products => _serviceClient.GetTableClient(_options.Tables.Product);
        public TableClient Orders => _serviceClient.GetTableClient(_options.Tables.Order);

        public static string NewId() => Guid.NewGuid().ToString("N");

        public async Task UpsertAsync<T>(TableClient client, T entity) where T : class, ITableEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await client.UpsertEntityAsync(entity);
        }

        public async Task<T?> GetAsync<T>(TableClient client, string partitionKey, string rowKey)
            where T : class, ITableEntity, new()
        {
            try
            {
                var response = await client.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task DeleteAsync(TableClient client, string partitionKey, string rowKey)
        {
            await client.DeleteEntityAsync(partitionKey, rowKey);
        }

        public IEnumerable<T> Query<T>(TableClient client, string? filter = null)
            where T : class, ITableEntity, new()
        {
            return client.Query<T>(filter);
        }
    }
}

