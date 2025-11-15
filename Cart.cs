using System;
using System.Collections.Generic;

namespace ABCRetail2.Models
{
    public class CartModel
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<CartItemModel> Items { get; set; } = new List<CartItemModel>();
    }

    public class CartItemModel
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } 
        public decimal Price { get; set; }       
        public int Quantity { get; set; }
    }
}