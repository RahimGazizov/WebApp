namespace Практика.Models
{
    public class Salary
    {
        public int Id { get; set; }
        public string Employee { get; set; }
        public int EmpoyeeId { get; set; }
        public double BaseSalary { get; set; }
        public int PurchaseCount {  get; set; }
        public int ProductionCount { get; set; }
        public int SaleCount { get; set; }
        public int TotalCount { get; set; }
        public double Bonus { get; set; }
        public double TotalSalary { get; set; }
        public string PayStatus { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
