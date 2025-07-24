namespace Практика.Models
{
    public class Materials
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UnitId { get; set; }
        public string Unit {  get; set; }
        public double Count { get; set; }
        public double Cost { get; set; }
    }
}
