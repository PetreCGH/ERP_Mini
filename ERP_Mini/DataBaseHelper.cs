using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Data;

namespace ERP_Mini
{
    internal class DataBaseHelper
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["ERPMini"].ConnectionString;
        public static SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            
            return conn;
        }

        public static bool AuthenticateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("AuthenticateUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    conn.Open();
                    int result = (int)cmd.ExecuteScalar();
                    return result == 1;
                }
            }
        }

        public static DataTable GetProducts()
        {
            DataTable dt = new DataTable();
            string query = "SELECT ProductID, Name, Price, Stock FROM Products";

            using (SqlConnection conn = GetConnection())
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.Fill(dt);
            }

            return dt;
        }

        public static DataTable GetCustomers()
        {
            DataTable dt = new DataTable();
            string query = "SELECT CustomerID, CustomerName, Email, Phone FROM Customers";

            using ( SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static bool AddProduct(string productName, decimal price, int stock)
        {
            string query = "INSERT INTO Products ([Name], Price, Stock) VALUES(@name, @price, @stock)";
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", productName);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public static bool EditProduct(int productId, string productName, decimal price, int stock)
        {
            string query = "UPDATE Products SET [Name]=@name, Price=@price, Stock=@stock WHERE ProductID=@ID";

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using(SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id",productId);
                    cmd.Parameters.AddWithValue("@name", productName);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;

                }
            }
        }

        public static bool DeleteProduct(int productId)
        {
            string query = "DELETE FROM Products WHERE ProductID=@id";
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", productId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                        
                }
            }
        }

        public static bool AddCustomer(string name, string email, string phone)
        {
            string query = "INSERT INTO Customers (CustomerName, Email, Phone) VALUES (@name, @email, @phone)";

            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", phone);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public static bool UpdateCustomer(int id, string name, string email, string phone)
        {
            string query = "UPDATE Customers SET CustomerName = @name, Email = @email, Phone = @phone WHERE CustomerID = @id";

            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public static bool DeleteCustomer(int customerId)
        {
            string query = "DELETE FROM Customers WHERE CustomerID = @id";

            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public static DataTable GetOrders()
        {
            DataTable dt = new DataTable();
            string query = "SELECT OrderID, CustomerID, OrderDate, TotalAmount, OrderStatus FROM Orders";

            using (SqlConnection conn = GetConnection())
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.Fill(dt);
            }

            return dt;
        }

        public static DataTable GetProductListForOrder()
        {
            DataTable dt = new DataTable();
            string query = "SELECT ProductID, Name, Price FROM Products";

            using (SqlConnection conn = GetConnection())
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
            {
                adapter.Fill(dt);
            }

            return dt;
        }

        public static decimal GetProductPrice(int productId)
        {
            decimal price = 0;
            string query = "SELECT Price FROM Products WHERE ProductID = @id";

            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", productId);
                conn.Open();

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    price = Convert.ToDecimal(result);
                }
            }

            return price;
        }

        public static int SaveOrder(int customerId, DateTime orderDate, string status, decimal totalAmount, List<(int ProductID, int Quantity, decimal Price)> orderItems)
        {
            int newOrderId = -1;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    
                    string insertOrderQuery = @"
                INSERT INTO Orders (CustomerID, OrderDate, TotalAmount, OrderStatus)
                VALUES (@customerId, @orderDate, @total, @status);
                SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdOrder = new SqlCommand(insertOrderQuery, conn, transaction);
                    cmdOrder.Parameters.AddWithValue("@customerId", customerId);
                    cmdOrder.Parameters.AddWithValue("@orderDate", orderDate);
                    cmdOrder.Parameters.AddWithValue("@total", totalAmount);
                    cmdOrder.Parameters.AddWithValue("@status", status);

                    newOrderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                    
                    foreach (var item in orderItems)
                    {
                        string insertDetailQuery = @"
                    INSERT INTO OrderDetails (OrderID, ProductID, Quantity, Price)
                    VALUES (@orderId, @productId, @qty, @price);";

                        SqlCommand cmdDetail = new SqlCommand(insertDetailQuery, conn, transaction);
                        cmdDetail.Parameters.AddWithValue("@orderId", newOrderId);
                        cmdDetail.Parameters.AddWithValue("@productId", item.ProductID);
                        cmdDetail.Parameters.AddWithValue("@qty", item.Quantity);
                        cmdDetail.Parameters.AddWithValue("@price", item.Price);

                        cmdDetail.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return newOrderId;
        }

        public static bool DeleteOrder(int orderId)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    
                    string deleteDetailsQuery = "DELETE FROM OrderDetails WHERE OrderID = @orderId";
                    SqlCommand cmdDetails = new SqlCommand(deleteDetailsQuery, conn, transaction);
                    cmdDetails.Parameters.AddWithValue("@orderId", orderId);
                    cmdDetails.ExecuteNonQuery();

                    
                    string deleteOrderQuery = "DELETE FROM Orders WHERE OrderID = @orderId";
                    SqlCommand cmdOrder = new SqlCommand(deleteOrderQuery, conn, transaction);
                    cmdOrder.Parameters.AddWithValue("@orderId", orderId);
                    cmdOrder.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public static DataTable GetOrderDetails(int orderId)
        {
            DataTable dt = new DataTable();
            string query = "SELECT ProductID, Quantity, Price FROM OrderDetails WHERE OrderID = @orderId";

            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@orderId", orderId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }

            return dt;
        }

        public static bool UpdateOrder(int orderId, int customerId, DateTime orderDate, string status, decimal totalAmount, List<(int ProductID, int Quantity, decimal Price)> orderItems)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    
                    string updateOrderQuery = @"
                UPDATE Orders
                SET CustomerID = @CustomerID,
                    OrderDate = @OrderDate,
                    OrderStatus = @Status,
                    TotalAmount = @TotalAmount
                WHERE OrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(updateOrderQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@CustomerID", customerId);
                        cmd.Parameters.AddWithValue("@OrderDate", orderDate);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        cmd.ExecuteNonQuery();
                    }

                    
                    string deleteDetailsQuery = "DELETE FROM OrderDetails WHERE OrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(deleteDetailsQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.ExecuteNonQuery();
                    }

                    
                    foreach (var item in orderItems)
                    {
                        string insertDetailQuery = @"
                    INSERT INTO OrderDetails (OrderID, ProductID, Quantity, Price)
                    VALUES (@OrderID, @ProductID, @Quantity, @Price)";

                        using (SqlCommand cmd = new SqlCommand(insertDetailQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@Price", item.Price);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw; 
                }
            }
        }





    }
}
