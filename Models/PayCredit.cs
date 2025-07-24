namespace Практика.Models
{
    public class PayCredit
    {
        public int Id { get; set; }
        public double Summa { get; set; }
        public double Pay { get; set; }
        public double Percent { get; set; }
        public double Fine { get; set; }
        public DateTime Date { get; set; }
        public int CreditId { get; set; }
        public double Credit { get; set; }
        public double Total_Summ { get; set; }
        public double Result {  get; set; }
        public int Expired {  get; set; }
    }
}
