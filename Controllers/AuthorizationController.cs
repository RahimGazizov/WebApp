using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using System.Data;
namespace Практика.Controllers
{
    public class AuthorizationController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;DataBase=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True;";

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(Authorization authorization)
        {
            int role;
            string name;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("LoginUser", connection))
                {
                    if(authorization.Login != null || authorization.Password != null)
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Login", authorization.Login);
                        command.Parameters.AddWithValue("@Password", authorization.Password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                name = reader.GetString(0);
                                role = reader.GetInt32(1);
                                HttpContext.Session.SetInt32("Role", role);
                                HttpContext.Session.SetString("Name", name);
                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                TempData["Error"] = "Не верный логин или пароль";
                                return View("Index");
                            }
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Введите или пароль";
                        return View("Index");
                    }
                }
            }
                return View();
        }
    }
}
