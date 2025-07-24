using Azure.Core.Pipeline;

namespace Практика.Models
{
    public class Employees
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PostId { get; set; }
        public string Post {  get; set; }
        public int Salary { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Login {  get; set; }
        public string Password { get; set; }
    }
}
