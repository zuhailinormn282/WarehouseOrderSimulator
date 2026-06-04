using System;
using System.Collections.Generic;

namespace WarehouseOrderSimulator
{
    // Product model
    public class Product
    {
        public int ProductId { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Inventory model
    public class InventoryItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    // Order model
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    // Order Item model - FIXED with proper properties
    public class OrderItem
    {
        public string SKU { get; set; }           // Product SKU
        public int Quantity { get; set; }          // Quantity ordered
        public decimal UnitPrice { get; set; }     // Price per unit
    }

    // Transaction Log model
    public class TransactionLog
    {
        public int LogId { get; set; }
        public string TransactionType { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
        public string OrderNumber { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}