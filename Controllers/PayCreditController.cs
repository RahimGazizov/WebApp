using Microsoft.AspNetCore.Mvc;
using Практика.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
namespace Практика.Controllers
{
    public class PayCreditController : Controller
    {
        private string connectionString = "Server=DESKTOP-9P53NEF\\SQLEXPRESS;DataBase=БД_СУБД;Trusted_Connection=True;TrustServerCertificate=True;";
        public IActionResult Index()
        {
            List<PayCredit> pay = new List<PayCredit>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT p.ID, p.DatePay,p.Pay,p.[Percent], p.Total_Summ,p.Summa,p.Expired,p.Fine,p.Result, c.summa " +
                    "FROM Pay p " +
                    "JOIN Credit c ON p.Credit_Id = c.ID ";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pay.Add(new PayCredit
                        {
                            Id = reader.GetInt32(0),
                            Date = reader.GetDateTime(1),
                            Pay = Math.Round(reader.GetDouble(2), 2),
                            Percent = Math.Round(reader.GetDouble(3), 2),
                            Total_Summ = Math.Round(reader.GetDouble(4), 2),
                            Summa = Math.Round(reader.GetDouble(5), 2),
                            Expired = reader.GetInt32(6),
                            Fine = Math.Round(reader.GetDouble(7), 2),
                            Result = Math.Round(reader.GetDouble(8), 6),
                            Credit = Math.Round(reader.GetDouble(9), 2),
                        });
                    }
                }
            }
            return View(pay);
        }
        public IActionResult Insert()
        {
            ViewBag.Credit = GetInfoCredit();
            return View();
        }
        [HttpPost]
        public IActionResult Insert(PayCredit pay)
        {
            PayCredit payList = new PayCredit();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("PayCredit", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CreditId", pay.CreditId);
                    command.Parameters.AddWithValue("@Date", pay.Date);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            payList = new PayCredit
                            {
                                Date = reader.GetDateTime(0),
                                Pay = Math.Round(reader.GetDouble(1), 2),
                                Percent = Math.Round(reader.GetDouble(2), 2),
                                Total_Summ = Math.Round(reader.GetDouble(3), 2),
                                Summa = Math.Round(reader.GetDouble(4), 2),
                                Expired = reader.GetInt32(5),
                                Fine = Math.Round(reader.GetDouble(6), 2),
                                Result = Math.Round(reader.GetDouble(7), 6),
                                Credit = Math.Round(reader.GetDouble(8), 2),
                                CreditId = pay.CreditId
                            };
                        }
                    }
                }
            }
            return View("CreditResult", payList);
        }
        [HttpPost]
        public IActionResult SavePayCredit(DateTime date, double pay, double percent, double total_summ, double summa, int expired, double fine, double result, int creditid)
        {
           
            double creditSumm = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (pay != null)
                    {
                        using (SqlCommand command = new SqlCommand("SavePayCredit", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@Date", date);
                            command.Parameters.AddWithValue("@Pay", pay);
                            command.Parameters.AddWithValue("@Percent", percent);
                            command.Parameters.AddWithValue("@Total_Summ", total_summ);
                            command.Parameters.AddWithValue("@Summa", summa);
                            command.Parameters.AddWithValue("@Expired", expired);
                            command.Parameters.AddWithValue("@Fine", fine);
                            command.Parameters.AddWithValue("@Result", result);
                            command.Parameters.AddWithValue("@CreditId", creditid);

                            command.ExecuteNonQuery();
                        }
                    }
                  
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                // Собираем модель из параметров метода
                var payModel = new PayCredit
                {
                    Date = date,
                    Pay = pay,
                    Percent = percent,
                    Total_Summ = total_summ,
                    Summa = summa,
                    Expired = expired,
                    Fine = fine,
                    Result = result,
                    CreditId = creditid
                };

                // Дополнительно загружаем сумму кредита, если нужно
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT summa FROM Credit WHERE ID=@Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", creditid);
                        var resultDb = command.ExecuteScalar();
                        if (resultDb != null)
                        {
                            payModel.Credit = Convert.ToDouble(resultDb);
                        }
                    }
                }

                TempData["Error"] = ex.Message;

                // Возвращаем View с заполненной моделью, чтобы форма осталась заполненной
                return View("CreditResult", payModel);
            }
        }

        private List<SelectListItem> GetInfoCredit()
        {
            List<SelectListItem> credit = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID, summa FROM Credit";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        credit.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetDouble(1).ToString()
                        });
                    }
                }
            }
            return credit;
        }
    }
}
