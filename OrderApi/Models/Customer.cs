using System.ComponentModel.DataAnnotations;

namespace OrderApi.Models
{
    public enum CustomerStatus
    {
        Active,
        Inactive,
        Blocked
    }

    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerCode { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = "";

        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = "";

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = "";

        [StringLength(500)]
        public string Address { get; set; } = "";

        [Range(0, double.MaxValue)]
        public decimal TotalSpent { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal CurrentDebt { get; set; } = 0;

        public CustomerStatus Status { get; set; } = CustomerStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<Order> Orders { get; set; } = new();

        public List<Debt> Debts { get; set; } = new();
    }
}