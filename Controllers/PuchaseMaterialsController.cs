using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
namespace Практика.Controllers
{
    public class PuchaseMaterialsController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;Database=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True";
        public IActionResult Index()
        {
            List<PuchaseMaterials> puchaseMaterials = new List<PuchaseMaterials>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT p.ID,m.name,p.count,p.price,p.date,e.name FROM Purchase_materials p " +
                    "JOIN Materials m ON p.material=m.ID " +
                    "JOIN Employees e ON p.employee=e.ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        puchaseMaterials.Add(new PuchaseMaterials
                        {
                            Id = reader.GetInt32(0),
                            Material = reader.GetString(1),
                            Count = reader.GetDouble(2),
                            Price = reader.GetDouble(3),
                            Date = reader.GetDateTime(4),
                            Employee = reader.GetString(5)
                        });
                    }
                }
            }
            ViewBag.Budget = GetBudget();
            return View(puchaseMaterials.OrderByDescending(p => p.Id).Take(5).ToList());
        }
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Purchase_materials WHERE ID=@Id";
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
            ViewBag.Materials = GetMaterials();
            ViewBag.Employees = GetEmployees();
            ViewBag.Budget = GetBudget();
            return View(new PuchaseMaterials());
        }
        [HttpPost]
        public IActionResult Insert(PuchaseMaterials puchase)
        {
            double budget = GetBudget();
            if (puchase.Price > budget)
            {
                ViewBag.Materials = GetMaterials();
                ViewBag.Employees = GetEmployees();
                ViewBag.Budget = GetBudget();
                TempData["ErrorMessage"] = "Не хватает бюджета для покупки";
                return View("Insert", puchase);
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Purchase_materials(material,count,price,date,employee) VALUES(@material,@count,@price,@date,@employee)";
                Console.WriteLine($"дата {puchase.Date}");
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@material", puchase.MaterialId);
                    command.Parameters.AddWithValue("@count", puchase.Count);
                    command.Parameters.AddWithValue("@price", puchase.Price);
                    command.Parameters.AddWithValue("@date", puchase.Date);
                    command.Parameters.AddWithValue("@employee", puchase.EmployeeId);
                    command.ExecuteNonQuery();
                }
                string updateMaterial = "UPDATE Materials SET count=count+@count, cost=cost+@cost WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(updateMaterial, connection))
                {
                    command.Parameters.AddWithValue("@count", puchase.Count);
                    command.Parameters.AddWithValue("@cost", puchase.Price);
                    command.Parameters.AddWithValue("@Id", puchase.MaterialId);
                    command.ExecuteNonQuery();
                }
                string updateBudget = "UPDATE Budget SET budget=budget-@budget WHERE ID=1";
                using (SqlCommand command = new SqlCommand(updateBudget, connection))
                {
                    command.Parameters.AddWithValue("@budget", puchase.Price);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            PuchaseMaterials puchase = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, material,count,price,date,employee FROM Purchase_materials WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            puchase = new PuchaseMaterials
                            {
                                Id = reader.GetInt32(0),
                                MaterialId = reader.GetInt32(1),
                                Count = reader.GetDouble(2),
                                Price = reader.GetDouble(3),
                                Date = reader.GetDateTime(4),
                                EmployeeId = reader.GetInt32(5)
                            };
                        }
                    }
                }
            }
            ViewBag.Materials = GetMaterials();
            ViewBag.Employees = GetEmployees();
            return View(puchase);
        }
        [HttpPost]
        public IActionResult Edit(PuchaseMaterials puchase)
        {
            double price = 0, count = 0, budget = GetBudget();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string getInfoMaterial = "SELECT count, price FROM Purchase_materials WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(getInfoMaterial, connection))
                {
                    command.Parameters.AddWithValue("@Id", puchase.Id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            count = reader.GetDouble(0);
                            price = reader.GetDouble(1);
                        }
                    }
                }

                if (puchase.Price > price || puchase.Price < price)
                {
                    if (puchase.Price > budget)
                    {
                        ViewBag.Materials = GetMaterials();
                        ViewBag.Employees = GetEmployees();
                        TempData["ErrorMessage"] = "Не хватает бюджета для этой покупки";
                        return RedirectToAction("Edit", puchase);

                    }
                }
                string query = "UPDATE Materials SET count=count-@oldcount+@newcount, cost=cost-@oldprice+@newprice WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", puchase.MaterialId);
                    command.Parameters.AddWithValue("@oldcount", count);
                    command.Parameters.AddWithValue("@newcount", puchase.Count);
                    command.Parameters.AddWithValue("@oldprice", price);
                    command.Parameters.AddWithValue("@newprice", puchase.Price);
                    command.ExecuteNonQuery();
                }
                if (puchase.Price > price || puchase.Price < price)
                {
                    string updateBudget = "UPDATE Budget SET budget=budget+@oldprice-@newprice WHERE ID=1";
                    using(SqlCommand command = new SqlCommand(updateBudget, connection))
                    {
                        command.Parameters.AddWithValue("@oldprice", price);
                        command.Parameters.AddWithValue("@newprice",puchase.Price);
                        command.ExecuteNonQuery();
                    }
                }
                string updatePurchase = "UPDATE Purchase_materials SET material=@material, count=@count, price=@price, date=@date, employee=@employee WHERE ID=@Id";
                using(SqlCommand command = new SqlCommand(updatePurchase, connection))
                {
                    command.Parameters.AddWithValue("@material",puchase.MaterialId);
                    command.Parameters.AddWithValue("@count",puchase.Count);
                    command.Parameters.AddWithValue("@price",puchase.Price);
                    command.Parameters.AddWithValue("@date",puchase.Date);
                    command.Parameters.AddWithValue("@employee",puchase.EmployeeId);
                    command.Parameters.AddWithValue("@Id",puchase.Id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public List<SelectListItem> GetMaterials()
        {
            List<SelectListItem> materials = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,name FROM Materials";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        materials.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return materials;
        }
        public List<SelectListItem> GetEmployees()
        {
            List<SelectListItem> employee = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,name FROM Employees";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employee.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return employee;
        }
        public double GetBudget()
        {
            double budget = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT budget FROM Budget WHERE ID=1";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    if (budget != null)
                    {
                        budget = Convert.ToDouble(result);
                    }
                }
            }
            return budget;
        }
    }
}
