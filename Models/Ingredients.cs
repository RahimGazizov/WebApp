using Microsoft.Identity.Client;

namespace Практика.Models
{
    public class Ingredients
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Product { get; set; }
        public int MaterialsId { get; set; }
        public string Materials { get; set; }
        public double Count { get; set; }
    }
}
