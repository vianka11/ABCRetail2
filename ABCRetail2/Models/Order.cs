namespace ABCRetail2.Models
{

    public class OrderEntity : TableBase
    {
        public string CustomerId { get; set; } = "";

        public string ProductId { get; set; } = "";

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }

    // Customer and product names
    public class OrderViewModel
    {
        public string RowKey { get; set; } = "";

        public string CustomerName { get; set; } = "";

        public string ProductName { get; set; } = "";

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total { get; set; }

        public string Status { get; set; } = "";

        public DateTime CreatedUtc { get; set; }
    }
}