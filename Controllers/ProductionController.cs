using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Serialization;
using System.Net.Security;
using Microsoft.AspNetCore.Mvc.Localization;
namespace Практика.Controllers
{
    public class ProductionController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;DataBase=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True;";
        public IActionResult Index()
        {
            List<Production> production = new List<Production>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT pr.ID,p.name,pr.count,pr.date,e.name FROM Production pr " +
                    "JOIN Employees e ON pr.employee = e.ID " +
                    "JOIN Products p ON pr.product = p.ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        production.Add(new Production
                        {
                            Id = reader.GetInt32(0),
                            Product = reader.GetString(1),
                            Count = reader.GetInt32(2),
                            Date = reader.GetDateTime(3),
                            Employee = reader.GetString(4)
                        });

                    }
                }

            }
            return View(production.OrderByDescending(p => p.Id).Take(7).ToList());
        }
        public IActionResult Insert()
        {
            ViewBag.Products = GetProduct();
            ViewBag.Employees = GetEmployee();
            return View();
        }
        [HttpPost]
        public IActionResult Insert(Production production)
        {
            Dictionary<int, double> materialsCount = GetMaterialsCount();
            Dictionary<int, double> materialsPrice = GetMaterialsPrice();

            Dictionary<int, double> ingredients = new Dictionary<int, double>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT material, count FROM Ingredients  " +
                    "WHERE product = @Product";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Product", production.ProductId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int materialId = reader.GetInt32(0);
                            double count = reader.GetDouble(1) * production.Count;
                            ingredients.Add(materialId, count);
                        }
                    }
                }
                List<string> materialError = new List<string>();
                foreach (var ingredient in ingredients)
                {
                    int key = ingredient.Key;
                    double count = ingredient.Value;
                    if (count > materialsCount[key])
                    {
                        string getMaterial = "SELECT name FROM Materials WHERE ID=@Id";
                        using (SqlCommand command = new SqlCommand(getMaterial, connection))
                        {
                            command.Parameters.AddWithValue("@Id", key);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    materialError.Add(reader.GetString(0));
                                }
                            }
                        }
                    }
                }
                if (materialError.Count > 0)
                {
                    TempData["ErrorMessage"] = $"Не хватает материала " + string.Join(", ", materialError);
                    ViewBag.Products = GetProduct();
                    ViewBag.Employees = GetEmployee();
                    return View(production);
                }
                double value = 0;
                double sum = 0;
                foreach (var ingred in ingredients)
                {
                    int key = ingred.Key;
                    double count = ingred.Value;
                    sum = materialsPrice[key] / materialsCount[key] * count;
                    value += sum;
                }
                string insertProduct = "UPDATE Products SET count=count+@Count, sum=sum+@Sum WHERE ID=@Product";
                using (SqlCommand command = new SqlCommand(insertProduct, connection))
                {
                    command.Parameters.AddWithValue("@Product", production.ProductId);
                    command.Parameters.AddWithValue("@Count", production.Count);
                    command.Parameters.AddWithValue("@Sum", value);
                    command.ExecuteNonQuery();
                }
                foreach (var ingre in ingredients)
                {
                    int key = ingre.Key;
                    double count = ingre.Value;
                    double cost = materialsPrice[key] / materialsCount[key] * count;
                    string updateMaterials = "UPDATE Materials SET count=count-@Count, cost=cost-@Cost WHERE ID=@Material";
                    using (SqlCommand command = new SqlCommand(updateMaterials, connection))
                    {
                        command.Parameters.AddWithValue("@Material", ingre.Key);
                        command.Parameters.AddWithValue("@Count", ingre.Value);
                        command.Parameters.AddWithValue("@Cost", cost);
                        command.ExecuteNonQuery();
                    }
                }
                string insertProduction = "INSERT INTO Production(product,count,date,employee) VALUES(@product,@count,@date,@employee)";
                using (SqlCommand command = new SqlCommand(insertProduction, connection))
                {
                    command.Parameters.AddWithValue("@product", production.ProductId);
                    command.Parameters.AddWithValue("@count", production.Count);
                    command.Parameters.AddWithValue("@date", production.Date);
                    command.Parameters.AddWithValue("@employee", production.EmployeeId);
                    command.ExecuteNonQuery();
                }

            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Production WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            Production production = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, product,count,date,employee FROM Production " +
                    "WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            production = new Production{
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
            ViewBag.Products = GetProduct();
            ViewBag.Employees = GetEmployee();
            return View(production);
        }
        [HttpPost]
        public IActionResult Edit(Production production)
        {
            Dictionary<int, double> materialsCount = GetMaterialsCount(); // количество материала
            Dictionary<int, double> materialsPrice = GetMaterialsPrice(); // цена материала
            int oldCount = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string GetOldCount = "SELECT count FROM Production WHERE ID=@Id";
                Console.WriteLine($"Айди {production.Id}");
                using (SqlCommand command = new SqlCommand(GetOldCount, connection))
                {
                    command.Parameters.AddWithValue("@Id", production.Id);
                    oldCount = (int)command.ExecuteScalar();
                }
                Dictionary<int, double> ingredients = new Dictionary<int, double>();
                Dictionary<int, double> Oldingredients = new Dictionary<int, double>();
                string getIngredientsInfo = "SELECT material, count FROM Ingredients WHERE Product=@ProductId";
                using (SqlCommand command = new SqlCommand(getIngredientsInfo, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", production.ProductId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int material = reader.GetInt32(0);
                            double newcount = reader.GetDouble(1) * production.Count;
                            double count = reader.GetDouble(1) * oldCount;
                            ingredients.Add(material, newcount);
                            Oldingredients.Add(material, count);
                        }
                    }
                }
                List<string> LackOfMaterials = new List<string>();
                foreach (var ingre in ingredients)
                {
                    int key = ingre.Key;
                    double count = ingre.Value;
                    if (count > materialsCount[key] + Oldingredients[key])
                    {
                        string getMaterialsName = "SELECT name FROM Materials WHERE ID=@Id";
                        using (SqlCommand command = new SqlCommand(getMaterialsName, connection))
                        {
                            command.Parameters.AddWithValue("@Id", key);
                            string name = (string)command.ExecuteScalar();
                            LackOfMaterials.Add(name);
                        }

                    }
                }
                if (LackOfMaterials.Count > 0)
                {
                    TempData["ErrorMessage"] = "Не хватает материала " + string.Join(", ", LackOfMaterials);
                    ViewBag.Products = GetProduct();
                    ViewBag.Employees = GetEmployee();
                    return View(production);
                }
                double OldPrice = 0; // старая цена
                double NewPrice = 0; // новая цена
                foreach (var ingre in ingredients)
                {
                    int key = ingre.Key;
                    double count = ingre.Value;
                    double oldPrice = 0;
                    double newPrice = 0;
                    oldPrice = materialsPrice[key] / materialsCount[key] * Oldingredients[key];
                    newPrice = materialsPrice[key] / materialsCount[key] * count;
                    OldPrice += oldPrice;
                    NewPrice += newPrice;
                }
                string UpdateProduct = "UPDATE Products SET count=count-@OldCount+@NewCount, sum=sum-@OldPrice+@NewPrice WHERE ID=@ProductId";
                using (SqlCommand command = new SqlCommand(UpdateProduct, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", production.ProductId);
                    command.Parameters.AddWithValue("@OldCount", oldCount);
                    command.Parameters.AddWithValue("@NewCount", production.Count);
                    command.Parameters.AddWithValue("@OldPrice", OldPrice);
                    command.Parameters.AddWithValue("@NewPrice", NewPrice);
                    command.ExecuteNonQuery();
                }
                foreach (var ingre in ingredients)
                {
                    int key = ingre.Key;
                    double count = ingre.Value;
                    double OldCost = materialsPrice[key] / materialsCount[key] * Oldingredients[key];
                    double NewCost = materialsPrice[key] / materialsCount[key] * count;
                    string updateMaterials = "UPDATE Materials SET count=count+@Oldcount-@Newcount, cost=cost+@Oldcost-@Newcost WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(updateMaterials, connection))
                    {
                        command.Parameters.AddWithValue("@Id", key);
                        command.Parameters.AddWithValue("@Oldcount", Oldingredients[key]);
                        command.Parameters.AddWithValue("@Newcount", count);
                        command.Parameters.AddWithValue("@Oldcost", OldCost);
                        command.Parameters.AddWithValue("@Newcost", NewCost);
                        command.ExecuteNonQuery();
                    }
                }
                string updateProduction = "UPDATE Production SET product=@product, count=@count,date=@date,employee=@employee WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(updateProduction, connection))
                {
                    command.Parameters.AddWithValue("@Id", production.Id);
                    command.Parameters.AddWithValue("@product", production.ProductId);
                    command.Parameters.AddWithValue("@count", production.Count);
                    command.Parameters.AddWithValue("@date", production.Date);
                    command.Parameters.AddWithValue("@employee", production.EmployeeId);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        private List<SelectListItem> GetProduct()
        {
            List<SelectListItem> products = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,name FROM Products";
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
        private List<SelectListItem> GetEmployee()
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
        private Dictionary<int, double> GetMaterialsCount()
        {
            Dictionary<int, double> materials = new Dictionary<int, double>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,count FROM Materials";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int Id = reader.GetInt32(0);
                        double count = reader.GetDouble(1);
                        materials.Add(Id, count);
                    }
                }

            }
            return materials;
        }
        private Dictionary<int, double> GetMaterialsPrice()
        {
            Dictionary<int, double> materials = new Dictionary<int, double>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,cost FROM Materials";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int Id = reader.GetInt32(0);
                        double count = reader.GetDouble(1);
                        materials.Add(Id, count);
                    }
                }

            }
            return materials;
        }
    }
}
