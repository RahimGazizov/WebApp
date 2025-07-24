using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Практика.Controllers
{
    public class SaleProductsController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;DataBase=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True;";
        public IActionResult Index()
        {
            List<SaleProducts> saleProducts = new List<SaleProducts>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("GetSaleProductsInfo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            saleProducts.Add(new SaleProducts
                            {
                                Id = reader.GetInt32(0),
                                ProductName = reader.GetString(1),
                                Count = reader.GetInt32(2),
                                Sum = reader.GetDouble(3),
                                Date = reader.GetDateTime(4),
                                EmployeeName = reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return View(saleProducts.OrderByDescending(p => p.Id).Take(7).ToList());
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("DeleteSaleProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Insert()
        {
            ViewBag.Products = GetProducts();
            ViewBag.Employees = GetEmployees();
            return View();
        }
        [HttpPost]
        public IActionResult Insert(SaleProducts saleProducts)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("InsertNewSaleProducts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProductId", saleProducts.ProductId);
                        command.Parameters.AddWithValue("@Count", saleProducts.Count);
                        command.Parameters.AddWithValue("@Date", saleProducts.Date);
                        command.Parameters.AddWithValue("@EmployeeId", saleProducts.EmployeeId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Products = GetProducts();
                ViewBag.Employees = GetEmployees();
                return View(saleProducts);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            SaleProducts saleProducts = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("GetSaleProductEdit", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            saleProducts = new SaleProducts
                            {
                                Id = reader.GetInt32(0),
                                ProductId = reader.GetInt32(1),
                                Count = reader.GetInt32(2),
                                Date = reader.GetDateTime(3),
                                EmployeeId = reader.GetInt32(4)
                            };
                        }
                    }
                }
            }
            ViewBag.Products = GetProducts();
            ViewBag.Employees = GetEmployees();
            return View(saleProducts);
        }
        [HttpPost]
        public IActionResult Edit(SaleProducts saleProducts)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("EditSaleProduct", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProductId", saleProducts.ProductId);
                        command.Parameters.AddWithValue("@Count", saleProducts.Count);
                        command.Parameters.AddWithValue("@Date", saleProducts.Date);
                        command.Parameters.AddWithValue("@EmployeeId", saleProducts.EmployeeId);
                        command.Parameters.AddWithValue("@Id", saleProducts.Id);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                ViewBag.Products = GetProducts();
                ViewBag.Employees = GetEmployees();
                TempData["Error"] = ex.Message;
                return View(saleProducts);
            }
        }
        private List<SelectListItem> GetProducts()
        {
            List<SelectListItem> products = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, name FROM Products";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return products;
        }
        private List<SelectListItem> GetEmployees()
        {
            List<SelectListItem> employees = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, name FROM Employees";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employees.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return employees;
        }
    }
}
