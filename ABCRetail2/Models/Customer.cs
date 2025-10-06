using Azure;
using Azure.Data.Tables;

namespace ABCRetail2.Models
{

    public abstract class TableBase : ITableEntity
    {
        public string PartitionKey { get; set; } = "";

        public string RowKey { get; set; } = "";

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }

    // Represents a customer stored in Azure Table Storage
    public class CustomerEntity : TableBase
    {
        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";
    }
}