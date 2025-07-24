using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Практика.Controllers
{
    public class MaterialsController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;Database=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True";
        public IActionResult Index()
        {
            List<Materials> materials = new List<Materials>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT m.ID,m.name, u.name,m.count,m.cost FROM Materials m " +
                    "JOIN UnitsOfMeasurement u ON m.unitOfMeasurement = u.ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        materials.Add(new Materials
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Unit = reader.GetString(2),
                            Count = Math.Round(reader.GetDouble(3), 2),
                            Cost = Math.Round(reader.GetDouble(4), 2)
                        });
                    }
                }
            }
            return View(materials);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Materials WHERE ID=@Id";
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
            return View(new Materials());
        }

        [HttpPost]
        public IActionResult Insert(Materials materials)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Materials(name,unitOfMeasurement,count,cost) VALUES(@name,@unitOfMeasurement,@count,@cost)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", materials.Name);
                    command.Parameters.AddWithValue("@unitOfMeasurement", materials.UnitId);
                    command.Parameters.AddWithValue("@count", materials.Count);
                    command.Parameters.AddWithValue("@cost", materials.Cost);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            Materials materials = new Materials();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,name, unitOfMeasurement,count,cost FROM Materials WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            materials.Id = reader.GetInt32(0);
                            materials.Name = reader.GetString(1);
                            materials.UnitId = reader.GetInt32(2);
                            materials.Count = Math.Round(reader.GetDouble(3), 2);
                            materials.Cost = Math.Round(reader.GetDouble(4), 2);
                        }
                    }
                }

            }
            ViewBag.Units = GetUnits();
            return View(materials);
        }
        [HttpPost]
        public IActionResult Edit(Materials materials)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Materials SET name=@name, unitOfMeasurement=@unitOfMeasurement, count=@count, cost=@cost WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", materials.Id);
                    command.Parameters.AddWithValue("@name", materials.Name);
                    command.Parameters.AddWithValue("@unitOfMeasurement", materials.UnitId);
                    command.Parameters.AddWithValue("@count", materials.Count);
                    command.Parameters.AddWithValue("@cost", materials.Cost);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public List<SelectListItem> GetUnits()
        {
            List<SelectListItem> units = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,name FROM UnitsOfMeasurement";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        units.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return units;
        }
    }
}
