using System.ComponentModel.DataAnnotations;

namespace OrderApi.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; } = "";

        [Range(0, double.MaxValue)]
        public double TotalAmount { get; set; }

        [Required]
        public List<OrderItem> Items { get; set; } = new();
    }
}