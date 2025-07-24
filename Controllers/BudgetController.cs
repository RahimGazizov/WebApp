using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Практика.Controllers
{
    public class BudgetController : Controller
    {
        public string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;DataBase=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True;";
        public IActionResult Index()
        {
            List<BudgetModel> budgets = new List<BudgetModel>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,budget,bonus, Precent FROM Budget";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new BudgetModel
                            {
                                Id = reader.GetInt32(0),
                                Budget = Math.Round(reader.GetDouble(1), 2),
                                Bonus = Math.Round(reader.GetDouble(2), 2),
                                Percent = Math.Round(reader.GetDouble(3), 2),
                            };
                            budgets.Add(item);
                        }
                    }
                }

            }
            return View(budgets);
        }
        public IActionResult Edit(int id)
        {
            BudgetModel budget = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID,budget,bonus, Precent FROM Budget WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            budget = new BudgetModel
                            {
                                Id = reader.GetInt32(0),
                                Budget = Math.Round(reader.GetDouble(1), 2),
                                Bonus = Math.Round(reader.GetDouble(2), 2),
                                Percent = Math.Round(reader.GetDouble(3), 2),
                            };
                        }
                    }
                }
            }
            return View(budget);
        }
        [HttpPost]
        public IActionResult Edit(double Budget,double Bonus,double Percent,int Id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Budget SET budget=@Budget, Precent=@Percent, bonus=@Bonus WHERE ID=@Id";
                Console.WriteLine($"{Budget}{Percent}{Bonus}{Id}");
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Budget", Budget);
                    command.Parameters.AddWithValue("@Percent", Percent);
                    command.Parameters.AddWithValue("@Bonus", Bonus);
                    command.Parameters.AddWithValue("@Id", Id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }

    }
}
