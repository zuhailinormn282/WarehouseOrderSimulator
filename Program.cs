using System;
using System.Collections.Generic;

namespace WarehouseOrderSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║     WAREHOUSE ORDER SIMULATOR - WMS Stock Allocator          ║
║                                                              ║
║     Simulating real warehouse order processing               ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            var db = new DatabaseHelper();

            // Test database connection
            Console.WriteLine("\n🔌 Testing database connection...");
            if (!db.TestConnection())
            {
                Console.WriteLine("❌ Cannot connect to database. Please check SQL Server is running.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("✅ Database connected successfully!\n");

            var processor = new OrderProcessor();

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n┌─────────────────────────────────────────────────┐");
                Console.WriteLine("│                    MAIN MENU                    │");
                Console.WriteLine("├─────────────────────────────────────────────────┤");
                Console.WriteLine("│  1. View Current Inventory                      │");
                Console.WriteLine("│  2. Process New Order                           │");
                Console.WriteLine("│  3. Process Demo Order (pre-configured)         │");
                Console.WriteLine("│  4. Exit                                        │");
                Console.WriteLine("└─────────────────────────────────────────────────┘");
                Console.Write("\nSelect option (1-4): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        processor.ShowInventory();
                        break;

                    case "2":
                        ProcessManualOrder(processor);
                        break;

                    case "3":
                        ProcessDemoOrder(processor);
                        break;

                    case "4":
                        running = false;
                        Console.WriteLine("\n👋 Thank you for using Warehouse Order Simulator!");
                        break;

                    default:
                        Console.WriteLine("❌ Invalid option. Please try again.");
                        break;
                }
            }
        }

        static void ProcessManualOrder(OrderProcessor processor)
        {
            Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("                   CREATE NEW ORDER");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            Console.Write("Enter Order Number (e.g., ORD-001): ");
            string orderNumber = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                Console.WriteLine("❌ Order number cannot be empty!");
                return;
            }

            var items = new List<OrderItem>();
            bool addingItems = true;

            while (addingItems)
            {
                Console.WriteLine("\n┌───────────────────────────────────────────────────────┐");
                Console.WriteLine("│ Available SKUs: SKU001, SKU002, SKU003, SKU004, SKU005 │");
                Console.WriteLine("└────────────────────────────────────────────────────────┘");
                Console.Write("Enter Product SKU (or 'done' to finish): ");
                string sku = Console.ReadLine()?.ToUpper();

                if (sku == "DONE")
                    break;

                if (string.IsNullOrWhiteSpace(sku))
                {
                    Console.WriteLine("❌ SKU cannot be empty!");
                    continue;
                }

                Console.Write($"Enter quantity for {sku}: ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("❌ Please enter a valid positive number!");
                    continue;
                }

                items.Add(new OrderItem { SKU = sku, Quantity = quantity });
                Console.WriteLine($"✅ Added {quantity} of {sku}");
            }

            if (items.Count == 0)
            {
                Console.WriteLine("❌ No items added to order!");
                return;
            }

            processor.ProcessOrder(orderNumber, items);
        }

        static void ProcessDemoOrder(OrderProcessor processor)
        {
            Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("                  PROCESSING DEMO ORDER");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            var demoItems = new List<OrderItem>
            {
                new OrderItem { SKU = "SKU001", Quantity = 2 },  // Laptop Stand
                new OrderItem { SKU = "SKU002", Quantity = 1 },  // Wireless Mouse
                new OrderItem { SKU = "SKU003", Quantity = 5 }    // USB-C Cable
            };

            string demoOrderNumber = $"DEMO-{DateTime.Now:yyyyMMddHHmmss}";
            processor.ProcessOrder(demoOrderNumber, demoItems);
        }
    }
}