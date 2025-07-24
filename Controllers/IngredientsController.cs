using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Практика.Controllers
{
    public class IngredientsController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;Database=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True";
        
        public IActionResult Index(int? productsId, int? editId)
        {
          
            List<Ingredients> ingredients = new List<Ingredients>();
            string query;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (productsId != null)
                {
                    query = "SELECT i.ID, m.name, i.count FROM Ingredients i " +
                        "JOIN Materials m ON i.material = m.ID " +
                        "WHERE product = @Product";
                }
                else
                {
                    query = "SELECT i.ID, p.name, m.name, i.count FROM Ingredients i " +
                       "JOIN Products p ON i.product = p.ID " +
                       "JOIN Materials m ON i.material = m.ID "; 
                }
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (productsId != null)
                    {
                        command.Parameters.AddWithValue("@Product", productsId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ingredients.Add(new Ingredients
                                {
                                    Id = reader.GetInt32(0),
                                    Materials = reader.GetString(1),
                                    Count = reader.GetDouble(2)
                                });
                            }
                        }
                    }
                    else
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ingredients.Add(new Ingredients
                                {
                                    Id = reader.GetInt32(0),
                                    Product = reader.GetString(1),
                                    Materials = reader.GetString(2),
                                    Count = reader.GetDouble(3)
                                });
                            }
                        }
                    }
                }
            }
            ViewBag.Products = GetProduct();
            ViewBag.Materials = GetMaterial();
            ViewBag.ProductID = productsId;

            if(editId != null)
            {
                Ingredients editingredients = null;
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string getInfo = "SELECT ID, product, material, count FROM Ingredients WHERE ID=@id";
                    using(SqlCommand command = new SqlCommand(getInfo, connection))
                    {
                        command.Parameters.AddWithValue("Id", editId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                editingredients = new Ingredients
                                {
                                    Id = reader.GetInt32(0),
                                    ProductId = reader.GetInt32(1),
                                    MaterialsId = reader.GetInt32(2),
                                    Count = reader.GetDouble(3)
                                };
                            }
                        }
                    }
                    
                }
                ViewBag.EditInfo = editingredients;
            }
          
            return View(ingredients);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            int product;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string getProductID = "SELECT product FROM Ingredients WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(getProductID, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    product = (int)command.ExecuteScalar();
                }
                string query = "DELETE FROM Ingredients WHERE ID=@Id";
                using(SqlCommand command  = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
                return RedirectToAction("Index", new { productsId = product});
            }
        }
        public IActionResult Insert(Ingredients ingredients)
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Ingredients WHERE product=@Product AND material=@Material";
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Product", ingredients.ProductId);
                    command.Parameters.AddWithValue("@Material", ingredients.MaterialsId);
                    int result = (int)command.ExecuteScalar();
                    if (result > 0)
                    {
                        TempData["ErrorMessage"] = "Такой ингредиент уже существует";
                        return RedirectToAction("Index", new { productsId = ingredients.ProductId });
                    }
                }
                string insertData = "INSERT INTO Ingredients(product,material,count) VALUES(@product,@material,@count)";
                using(SqlCommand command = new SqlCommand(insertData, connection))
                {
                    command.Parameters.AddWithValue("@product", ingredients.ProductId);
                    command.Parameters.AddWithValue("@material", ingredients.MaterialsId);
                    command.Parameters.AddWithValue("@count", ingredients.Count);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index", new { productsId = ingredients.ProductId });

        }
        [HttpPost]
        public IActionResult Edit(Ingredients ingredients)
        {
            int count;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string queryCheck = "SELECT COUNT(*) FROM Ingredients WHERE product=@product AND material=@material AND ID!=@id";
                using (SqlCommand command = new SqlCommand(queryCheck, connection))
                {
                    command.Parameters.AddWithValue("@Id", ingredients.Id);
                    command.Parameters.AddWithValue("@product", ingredients.ProductId);
                    command.Parameters.AddWithValue("@material", ingredients.MaterialsId);
                    count = (int)command.ExecuteScalar();
                    if (count > 0)
                    {
                        TempData["ErrorMessage"] = "Такой ингридиент уже есть";
                        return RedirectToAction("Index", new { productsId = ingredients.ProductId, editId = ingredients.Id });
                    }
                }
                string query = "UPDATE Ingredients SET product = @product, material = @material, count = @count WHERE ID=@Id";
                Console.WriteLine($"продукт {ingredients.ProductId} материал {ingredients.ProductId} кол-во {ingredients.ProductId} айди{ingredients.Id}");
                using (SqlCommand command = new SqlCommand(@query, connection))
                {
                    command.Parameters.AddWithValue("@product", ingredients.ProductId);
                    command.Parameters.AddWithValue("@material", ingredients.MaterialsId);
                    command.Parameters.AddWithValue("@count", ingredients.Count);
                    command.Parameters.AddWithValue("@Id", ingredients.Id);
                    command.ExecuteNonQuery();


                }
            }
            return RedirectToAction("Index", new { productsId = ingredients.ProductId });
        }
        public List<SelectListItem> GetProduct()
        {
            List<SelectListItem> product = new List<SelectListItem>();
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,name FROM Products";
                using(SqlCommand command = new SqlCommand(query, connection))
                using(SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        product.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return product;
        }
        public List<SelectListItem> GetMaterial()
        {
            List<SelectListItem> material = new List<SelectListItem>();
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,name FROM Materials";
                using(SqlCommand command = new SqlCommand(query, connection))
                using(SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        material.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return material;
        }
    }
}
