using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace WarehouseOrderSimulator
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Test database connection
        public bool TestConnection()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }

        // Get all products with inventory
        public List<InventoryItem> GetInventory()
        {
            var inventory = new List<InventoryItem>();
            var query = @"
                SELECT p.ProductId, p.Name, i.Quantity 
                FROM Products p
                JOIN Inventory i ON p.ProductId = i.ProductId";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                inventory.Add(new InventoryItem
                {
                    ProductId = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    Quantity = reader.GetInt32(2)
                });
            }
            return inventory;
        }

        // Check stock for a specific product
        public int GetProductStock(int productId)
        {
            var query = "SELECT Quantity FROM Inventory WHERE ProductId = @ProductId";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProductId", productId);
            connection.Open();
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Update inventory quantity
        public bool UpdateInventory(int productId, int newQuantity)
        {
            var query = "UPDATE Inventory SET Quantity = @Quantity, LastUpdated = GETDATE() WHERE ProductId = @ProductId";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Quantity", newQuantity);
            command.Parameters.AddWithValue("@ProductId", productId);
            connection.Open();
            return command.ExecuteNonQuery() > 0;
        }

        // Create a new order
        public int CreateOrder(string orderNumber, decimal totalAmount)
        {
            var query = "INSERT INTO Orders (OrderNumber, Status, TotalAmount) VALUES (@OrderNumber, 'Pending', @TotalAmount); SELECT SCOPE_IDENTITY();";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@OrderNumber", orderNumber);
            command.Parameters.AddWithValue("@TotalAmount", totalAmount);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }

        // Add items to order
        public void AddOrderItem(int orderId, int productId, int quantity, decimal unitPrice)
        {
            var query = "INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.Parameters.AddWithValue("@Quantity", quantity);
            command.Parameters.AddWithValue("@UnitPrice", unitPrice);
            connection.Open();
            command.ExecuteNonQuery();
        }

        // Update order status
        public void UpdateOrderStatus(int orderId, string status)
        {
            var query = "UPDATE Orders SET Status = @Status WHERE OrderId = @OrderId";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@OrderId", orderId);
            connection.Open();
            command.ExecuteNonQuery();
        }

        // Log transaction
        public void LogTransaction(string transactionType, int? productId, int? quantity, string orderNumber, string message)
        {
            var query = @"
                INSERT INTO TransactionLogs (TransactionType, ProductId, Quantity, OrderNumber, Message) 
                VALUES (@Type, @ProductId, @Quantity, @OrderNumber, @Message)";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Type", transactionType);
            command.Parameters.AddWithValue("@ProductId", (object?)productId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Quantity", (object?)quantity ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderNumber", (object?)orderNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@Message", message);
            connection.Open();
            command.ExecuteNonQuery();
        }

        // Get product ID by SKU
        public int? GetProductIdBySku(string sku)
        {
            var query = "SELECT ProductId FROM Products WHERE SKU = @SKU";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SKU", sku);
            connection.Open();
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : (int?)null;
        }

        // Get product details by SKU - FIXED VERSION
        public Product GetProductBySku(string sku)
        {
            if (string.IsNullOrEmpty(sku))
                return null;

            var query = "SELECT ProductId, SKU, Name, Price FROM Products WHERE SKU = @SKU";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SKU", sku);
            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Product
                {
                    ProductId = reader.GetInt32(0),
                    SKU = reader.GetString(1),
                    Name = reader.GetString(2),
                    Price = reader.GetDecimal(3)
                };
            }
            return null;
        }
    }
}