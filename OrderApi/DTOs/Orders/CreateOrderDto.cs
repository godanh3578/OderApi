namespace OrderApi.DTOs.Orders
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public List<CreateOrderDetailDto> Items { get; set; } = new();
        public decimal DiscountAmount { get; set; }
        public string CreatedBy { get; set; } = "";
    }

    public class CreateOrderDetailDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
