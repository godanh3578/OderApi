using System.ComponentModel.DataAnnotations;

namespace OrderApi.Models
{
    public enum OrderStatus
    {
        Pending,      // Đơn hàng mới tạo, chưa hoàn tất thanh toán
        Confirmed,    // Đơn hàng đã được xác nhận
        Paid,         // Đơn hàng đã thanh toán đủ
        Debt,         // Đơn hàng còn công nợ
        Cancelled     // Đơn hàng đã bị hủy
    }

    public enum PaymentStatus
    {
        Unpaid,   // Chưa thanh toán
        Partial,  // Thanh toán một phần
        Paid      // Đã thanh toán đủ
    }

    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderCode { get; set; } = "";

        [StringLength(100)]
        public string? IdempotencyKey { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = "";

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal FinalAmount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal DebtAmount { get; set; } = 0;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        [Required]
        public List<OrderDetail> Items { get; set; } = new();

        public List<Payment> Payments { get; set; } = new();

        public Debt? Debt { get; set; }
    }
}