using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Практика.Models;
namespace Практика.Controllers
{
    public class EmployeesController : Controller
    {
        private string connactionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;DataBase=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True;";
        public IActionResult Index()
        {
            List<Employees> employees = new List<Employees>();
            using (SqlConnection connection = new SqlConnection(connactionString))
            {
                connection.Open();
                string query = "SELECT e.ID,e.name,p.post,e.salary,e.address,e.phone,e.Login,e.Password " +
                    "FROM Employees e " +
                    "JOIN Positions p ON e.post=p.ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employees.Add(new Employees
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Post = reader.GetString(2),
                            Salary = reader.GetInt32(3),
                            Address = reader.GetString(4),
                            Phone = reader.GetString(5),
                            Login = reader.GetString(6),
                            Password = reader.GetString(7),
                        });
                    }
                }
            }
            return View(employees);
        }
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connactionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Employees WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
        public IActionResult Insert()
        {
            ViewBag.Post = GetPost();
            return View();
        }
        [HttpPost]
        public IActionResult Insert(Employees employees)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connactionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Employees(name,post,salary,address,phone,Login,Password) " +
                        "VALUES(@name,@post,@salary,@address,@phone,@Login,@Password)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", employees.Name);
                        command.Parameters.AddWithValue("@post", employees.PostId);
                        command.Parameters.AddWithValue("@salary", employees.Salary);
                        command.Parameters.AddWithValue("@address", employees.Address);
                        command.Parameters.AddWithValue("@phone", employees.Phone);
                        command.Parameters.AddWithValue("@Login", employees.Login);
                        command.Parameters.AddWithValue("@Password", employees.Password);
                        command.ExecuteNonQuery();
                    }
                }
            }
           
            catch(SqlException ex)
            {
                TempData["Error"] = "Ошибка выполнения" + ex;
                ViewBag.Post = GetPost();
                return View("Insert", employees);


            }
            return RedirectToAction("Index");
        }
        private List<SelectListItem> GetPost()
        {
            List<SelectListItem> post = new List<SelectListItem>();
            using(SqlConnection connection = new SqlConnection(connactionString))
            {
                connection.Open();
                string query = "SELECT ID,post FROM Positions";
                using(SqlCommand command = new SqlCommand(query, connection))
                    using(SqlDataReader  reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        post.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }
            return post;
        }
        public IActionResult Edit(int id)
        {
            Employees employees = null;
            using (SqlConnection connection = new SqlConnection(connactionString))
            {
                connection.Open();
                string query = "SELECT ID,name,post,salary,address,phone,Login,Password FROM Employees " +
                    "WHERE ID=@Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            employees = new Employees
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                PostId = reader.GetInt32(2),
                                Salary = reader.GetInt32(3),
                                Address = reader.GetString(4),
                                Phone = reader.GetString(5),
                                Login = reader.GetString(6),
                                Password = reader.GetString(7),
                            };
                        }
                    }
                }
            }
            ViewBag.Post = GetPost();
            return View(employees);
        }
        [HttpPost]
        public IActionResult Edit(Employees employees)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connactionString))
                {
                    connection.Open();
                    string query = "UPDATE Employees SET name=@name,post=@post,salary=@salary,address=@address,phone=@phone,Login=@Login,Password=@Password " +
                        "WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", employees.Name);
                        command.Parameters.AddWithValue("@post", employees.PostId);
                        command.Parameters.AddWithValue("@salary", employees.Salary);
                        command.Parameters.AddWithValue("@address", employees.Address);
                        command.Parameters.AddWithValue("@phone", employees.Phone);
                        command.Parameters.AddWithValue("@Login", employees.Login);
                        command.Parameters.AddWithValue("@Password", employees.Password);
                        command.Parameters.AddWithValue("@Id", employees.Id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Ошибка выполнения" + ex;
                ViewBag.Post = GetPost();
                return View("Insert", employees);


            }
            return RedirectToAction("Index");
        }
    }
}
