using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using System.Data;
namespace Практика.Controllers
{
    public class CreditController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;DataBase=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True;";
        public IActionResult Index()
        {
            List<Credit> credit = new List<Credit>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("CreditInfo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            credit.Add(new Credit
                            {
                                Id = reader.GetInt32(0),
                                Summa = reader.GetDouble(1),
                                Year = reader.GetInt32(2),
                                Precent = reader.GetDouble(3),
                                Fine = reader.GetDouble(4),
                                Date = reader.GetDateTime(5),
                                Description = reader.GetString(6)
                            });
                           
                        }
                    }
                }
            }
            return View(credit);
        }
        public IActionResult Delete(int id)
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Credit WHERE ID=@Id";
                using(SqlCommand command = new SqlCommand(query, connection)) 
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult Insert()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Insert(Credit credit)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using(SqlCommand command = new SqlCommand("AddCredit",connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Summa", credit.Summa);
                    command.Parameters.AddWithValue("@Year", credit.Year);
                    command.Parameters.AddWithValue("@Percent", credit.Precent);
                    command.Parameters.AddWithValue("@Fine", credit.Fine);
                    command.Parameters.AddWithValue("@Date", credit.Date);
                    command.Parameters.AddWithValue("@Description", credit.Description);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
    }
}
