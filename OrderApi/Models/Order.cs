using System.ComponentModel.DataAnnotations;

namespace OrderApi.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Range(0, 100)]
        public double Discount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public double TotalAmount { get; set; }

        [Required]
        public List<OrderItem> Items { get; set; } = new();
    }
}