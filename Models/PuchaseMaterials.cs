namespace Практика.Models
{
    public class PuchaseMaterials
    {
        public int Id { get; set; }
        public string Material {  get; set; }
        public int MaterialId { get; set; }
        public double Count { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string Employee {  get; set; }
        public int EmployeeId { get; set; }
    }
}
