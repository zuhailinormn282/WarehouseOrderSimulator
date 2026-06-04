using System;
using System.Collections.Generic;
using System.Linq;

namespace WarehouseOrderSimulator
{
    public class OrderProcessor
    {
        private readonly DatabaseHelper _db;

        public OrderProcessor()
        {
            _db = new DatabaseHelper();
        }

        // Process an order - this is the core WMS logic
        public bool ProcessOrder(string orderNumber, List<OrderItem> items)
        {
            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine($"Processing Order: {orderNumber}");
            Console.WriteLine($"Order Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"{new string('=', 60)}\n");

            // Step 1: Check stock availability for all items
            var stockCheck = new List<(OrderItem item, int availableStock, Product product)>();
            bool allInStock = true;

            foreach (var item in items)
            {
                var product = _db.GetProductBySku(item.SKU);
                if (product == null)
                {
                    Console.WriteLine($"❌ ERROR: Product with SKU '{item.SKU}' not found!");
                    _db.LogTransaction("ORDER_FAILED", null, item.Quantity, orderNumber, $"Product not found: {item.SKU}");
                    return false;
                }

                int currentStock = _db.GetProductStock(product.ProductId);
                item.UnitPrice = product.Price;

                if (currentStock >= item.Quantity)
                {
                    stockCheck.Add((item, currentStock, product));
                    Console.WriteLine($"✅ {product.Name} (SKU: {item.SKU}) - Requested: {item.Quantity}, Available: {currentStock}");
                }
                else
                {
                    allInStock = false;
                    Console.WriteLine($"❌ {product.Name} (SKU: {item.SKU}) - Requested: {item.Quantity}, Available: {currentStock} - INSUFFICIENT STOCK!");
                    _db.LogTransaction("ORDER_FAILED", product.ProductId, item.Quantity, orderNumber, $"Insufficient stock: requested {item.Quantity}, available {currentStock}");
                }
            }

            if (!allInStock)
            {
                Console.WriteLine($"\n❌ ORDER REJECTED: Insufficient stock for some items.");
                return false;
            }

            // Step 2: Calculate total amount
            decimal totalAmount = stockCheck.Sum(x => x.item.Quantity * x.item.UnitPrice);
            Console.WriteLine($"\n💰 Total Amount: ${totalAmount:F2}");

            // Step 3: Create order in database
            int orderId = _db.CreateOrder(orderNumber, totalAmount);
            Console.WriteLine($"📝 Order created with ID: {orderId}");

            // Step 4: Reduce inventory and add order items
            Console.WriteLine($"\n📦 Updating inventory...");
            foreach (var (item, availableStock, product) in stockCheck)
            {
                int newStock = availableStock - item.Quantity;
                _db.UpdateInventory(product.ProductId, newStock);
                _db.AddOrderItem(orderId, product.ProductId, item.Quantity, item.UnitPrice);
                _db.LogTransaction("STOCK_DEDUCTION", product.ProductId, item.Quantity, orderNumber, $"Deducted {item.Quantity} from {product.Name}. New stock: {newStock}");
                Console.WriteLine($"   - {product.Name}: {availableStock} → {newStock}");
            }

            // Step 5: Update order status to Completed
            _db.UpdateOrderStatus(orderId, "Completed");
            _db.LogTransaction("ORDER_COMPLETED", null, null, orderNumber, $"Order completed successfully. Total: ${totalAmount:F2}");

            // Step 6: Print receipt
            PrintReceipt(orderNumber, stockCheck, totalAmount);

            return true;
        }

        private void PrintReceipt(string orderNumber, List<(OrderItem item, int availableStock, Product product)> items, decimal totalAmount)
        {
            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine($"                  RECEIPT");
            Console.WriteLine($"{new string('=', 60)}");
            Console.WriteLine($"Order Number: {orderNumber}");
            Console.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"\n{"Item",-25} {"Qty",-10} {"Unit Price",-12} {"Total"}");
            Console.WriteLine($"{new string('-', 60)}");

            foreach (var (item, _, product) in items)
            {
                decimal lineTotal = item.Quantity * item.UnitPrice;
                Console.WriteLine($"{product.Name,-25} {item.Quantity,-10} ${item.UnitPrice:F2,-11} ${lineTotal:F2}");
            }

            Console.WriteLine($"{new string('-', 60)}");
            Console.WriteLine($"{"TOTAL",-47} ${totalAmount:F2}");
            Console.WriteLine($"{new string('=', 60)}");
            Console.WriteLine($"Thank you for your order!");
            Console.WriteLine($"{new string('=', 60)}\n");
        }

        // Display current inventory
        public void ShowInventory()
        {
            var inventory = _db.GetInventory();
            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine($"CURRENT INVENTORY STATUS");
            Console.WriteLine($"{new string('=', 60)}");
            Console.WriteLine($"{"Product Name",-30} {"Quantity",-10}");
            Console.WriteLine($"{new string('-', 60)}");

            foreach (var item in inventory)
            {
                Console.WriteLine($"{item.ProductName,-30} {item.Quantity,-10}");
            }
            Console.WriteLine();
        }

        // Show recent transactions
        public void ShowRecentTransactions()
        {
            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine($"RECENT TRANSACTIONS");
            Console.WriteLine($"{new string('=', 60)}");
            Console.WriteLine("Check TransactionLogs table in SSMS for full history");
        }
    }
}