namespace OrderApi.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Customers Customer { get; set; } = null!;
    }
}