using System.ComponentModel.DataAnnotations;

namespace OrderApi.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = "";

        [Phone]
        public string Phone { get; set; } = "";

        [EmailAddress]
        public string Email { get; set; } = "";

        public string Address { get; set; } = "";

        [Range(0, double.MaxValue)]
        public double Debt { get; set; } = 0;

        public List<Order> Orders { get; set; } = new();
    }
}