namespace OrderApi.Services
{
    public interface IProductCatalogClient
    {
        Task<ProductCatalogItem?> GetProductAsync(int productId, CancellationToken cancellationToken = default);
    }

    public sealed class ProductCatalogItem
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal SellingPrice { get; set; }
        public int QuantityAvailable { get; set; }
        public string StockStatus { get; set; } = "";
    }
}
