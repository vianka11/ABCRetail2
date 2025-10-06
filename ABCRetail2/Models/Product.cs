using Azure;
using Azure.Data.Tables;

namespace ABCRetail2.Models
{

    public class ProductEntity : TableBase
    {
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = "";

        public int StockQuantity { get; set; }
    }
}