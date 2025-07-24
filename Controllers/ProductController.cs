using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Практика.Controllers
{
    public class ProductController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;Database=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True";

        public IActionResult Index()
        {
            List<Product> products = new List<Product>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT p.ID,p.name,u.name,p.count,ROUND(p.sum,4) FROM Products p " +
                    "JOIN UnitsOfMeasurement u ON p.unitOfmeasurement = u.ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Unit = reader.GetString(2),
                            Count = reader.GetInt32(3),
                            Price = reader.GetDouble(4)
                        });
                    }
                }
            }
            return View(products);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "Delete FROM Products WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Insert()
        {
            ViewBag.Units = GetUnits();
            return View(new Product());
        }
        [HttpPost]
        public IActionResult Insert(Product product)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Products(name,unitOfmeasurement,count,sum) VALUES(@name,@unitOfmeasurement,@count,@sum)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", product.Name);
                    command.Parameters.AddWithValue("@unitOfmeasurement", product.UnitId);
                    command.Parameters.AddWithValue("@count", product.Count);
                    command.Parameters.AddWithValue("@sum", product.Price);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public List<SelectListItem> GetUnits()
        {
            List<SelectListItem> unit = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, name FROM UnitsOfMeasurement";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        unit.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return unit;
        }
        public IActionResult Edit(int id)
        {
            Product product = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,name,unitOfmeasurement,count,sum FROM Products WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                UnitId = reader.GetInt32(2),
                                Count = reader.GetInt32(3),
                                Price = reader.GetDouble(4)
                            };
                        }
                    }
                }
            }
            ViewBag.Units = GetUnits();
            return View(product);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Products SET name=@name, unitOfmeasurement=@unitOfmeasurement,count=@count, sum=@sum WHERE ID=@Id";
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", product.Id);
                    command.Parameters.AddWithValue("@name", product.Name);
                    command.Parameters.AddWithValue("@unitOfmeasurement", product.UnitId);
                    command.Parameters.AddWithValue("@count", product.Count);
                    command.Parameters.AddWithValue("@sum", product.Price);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
    }
}