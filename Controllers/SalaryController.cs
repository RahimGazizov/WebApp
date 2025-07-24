using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using Практика.Models;
namespace Практика.Controllers
{
    public class SalaryController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;DataBase=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True;";
        public IActionResult Index(Salary salary)
        {
            List<Salary> salaryList = new List<Salary>();
            double total_Sum = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("CalculateSalary", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (salary.Year > 0 && salary.Month > 0)
                    {
                        command.Parameters.AddWithValue("@Year", salary.Year);
                        command.Parameters.AddWithValue("@Month", salary.Month);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                salaryList.Add(new Salary
                                {
                                    Id = reader.GetInt32(0),
                                    Employee = reader.GetString(1),
                                    BaseSalary = reader.GetDouble(2),
                                    PurchaseCount = reader.GetInt32(3),
                                    ProductionCount = reader.GetInt32(4),
                                    SaleCount = reader.GetInt32(5),
                                    TotalCount = reader.GetInt32(6),
                                    Bonus = reader.GetDouble(7),
                                    TotalSalary = reader.GetDouble(8),
                                    PayStatus = reader.GetBoolean(9) ? "выплачено" : "невыплачено",
                                });
                            }
                            if (reader.NextResult() && reader.Read())
                            {
                                total_Sum = reader.GetDouble(0);
                            }
                        }
                    }

                }

            }
            TempData["Year"] = salary.Year;
            TempData["Month"] = salary.Month;
            ViewBag.Years = GetYear();
            ViewBag.Months = GetMonth();
            ViewBag.Total_Sum = total_Sum;
            return View(salaryList);
        }
        public IActionResult Edit(int id)
        {
            Salary salary = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("EditSalary", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Total_salary", DBNull.Value);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salary = new Salary
                            {
                                Id = reader.GetInt32(0),
                                Employee = reader.GetString(1),
                                TotalSalary = reader.GetDouble(2),
                                Year = reader.GetInt32(3),
                                Month = reader.GetInt32(4)
                            };
                        }

                    }
                }
            }
            return View(salary);
        }
        [HttpPost]
        public IActionResult Edit(Salary salary)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("EditSalary", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Total_salary", salary.TotalSalary);
                    command.Parameters.AddWithValue("@Id", salary.Id);
                    command.ExecuteNonQuery();
                }
            }
            TempData["Year"] = salary.Year;
            TempData["Month"] = salary.Month;
            return RedirectToAction("Index", new { Year = TempData["Year"], Month = TempData["Month"] });
        }
        [HttpPost]
        public IActionResult PaySalaries(Salary salary)
        {
            
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("PaySalaries", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                            Console.WriteLine($"year {salary.Year} month {salary.Month}");
                        if(salary.Year > 0 &&  salary.Month > 0)
                        {
                            command.Parameters.AddWithValue("@Year", salary.Year);
                            command.Parameters.AddWithValue("@Month", salary.Month);
                            command.ExecuteNonQuery();
                        }
                       
                    }
                }
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }
           
            TempData["Year"] = salary.Year;
            TempData["Month"] = salary.Month;
            return RedirectToAction("Index", new { Year = TempData["Year"], Month = TempData["Month"] });
        }
        private List<SelectListItem> GetYear()
        {
            List<SelectListItem> year = new List<SelectListItem>();
            for (int i = 2024; i <= 2030; i++)
            {
                year.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                });
            }
            return year;
        }
        private List<SelectListItem> GetMonth()
        {
            List<SelectListItem> month = new List<SelectListItem>();
            string[] monthName = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            for (int i = 0; i < monthName.Length; i++)
            {
                month.Add(new SelectListItem
                {
                    Value = (i + 1).ToString(),
                    Text = monthName[i].ToString()
                });
            }
            return month;
        }
    }
}
