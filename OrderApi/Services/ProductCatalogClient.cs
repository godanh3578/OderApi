using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;

namespace OrderApi.Services
{
    public sealed class ProductCatalogClient : IProductCatalogClient
    {
        private readonly HttpClient _httpClient;
        private readonly OrderDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly ILogger<ProductCatalogClient> _logger;

        public ProductCatalogClient(
            HttpClient httpClient,
            OrderDbContext dbContext,
            IConfiguration config,
            ILogger<ProductCatalogClient> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _config = config;
            _logger = logger;
        }

        public async Task<ProductCatalogItem?> GetProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            if (_config.GetValue<bool>("ProductIntegration:UseGatewayLookup"))
            {
                var product = await TryGetFromGatewayAsync(productId, cancellationToken);
                if (product != null)
                    return product;

                if (!_config.GetValue("ProductIntegration:AllowCacheFallback", true))
                    return null;
            }

            return await GetFromCacheAsync(productId, cancellationToken);
        }

        private async Task<ProductCatalogItem?> TryGetFromGatewayAsync(int productId, CancellationToken cancellationToken)
        {
            if (_httpClient.BaseAddress == null)
                return null;

            var template = _config["ProductIntegration:ProductDetailPath"] ?? "/api/products/{id}";
            var path = template
                .Replace("{id}", productId.ToString(CultureInfo.InvariantCulture))
                .Replace("{productId}", productId.ToString(CultureInfo.InvariantCulture));

            try
            {
                using var response = await _httpClient.GetAsync(path, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Product API returned {StatusCode} for ProductId={ProductId}",
                        response.StatusCode,
                        productId);
                    return null;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                return MapGatewayProduct(document.RootElement, productId);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "Cannot get ProductId={ProductId} from Product API", productId);
                return null;
            }
        }

        private async Task<ProductCatalogItem?> GetFromCacheAsync(int productId, CancellationToken cancellationToken)
        {
            var stock = await _dbContext.ProductStockCaches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

            if (stock == null)
                return null;

            return new ProductCatalogItem
            {
                ProductId = stock.ProductId,
                ProductCode = stock.ProductCode,
                ProductName = stock.ProductName,
                CategoryName = stock.CategoryName,
                SellingPrice = stock.SellingPrice,
                QuantityAvailable = stock.QuantityAvailable,
                StockStatus = stock.StockStatus.ToString()
            };
        }

        private static ProductCatalogItem MapGatewayProduct(JsonElement element, int requestedProductId)
        {
            return new ProductCatalogItem
            {
                ProductId = GetInt(element, requestedProductId, "productId", "id", "ProductID", "Id"),
                ProductCode = GetString(element, "productCode", "code", "ProductCode", "Code"),
                ProductName = GetString(element, "productName", "name", "ProductName", "Name"),
                CategoryName = GetString(element, "categoryName", "category", "CategoryName", "Category"),
                SellingPrice = GetDecimal(element, "sellingPrice", "price", "Price", "SellingPrice"),
                QuantityAvailable = GetInt(element, 0, "quantityAvailable", "quality", "stock", "availableStock", "QuantityAvailable", "Quantity"),
                StockStatus = GetString(element, "stockStatus", "status", "StockStatus", "Status")
            };
        }

        private static string GetString(JsonElement element, params string[] names)
        {
            foreach (var name in names)
            {
                if (TryGetProperty(element, name, out var property))
                    return property.ValueKind == JsonValueKind.String ? property.GetString() ?? "" : property.ToString();
            }

            return "";
        }

        private static int GetInt(JsonElement element, int fallback, params string[] names)
        {
            foreach (var name in names)
            {
                if (!TryGetProperty(element, name, out var property))
                    continue;

                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
                    return value;

                if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out value))
                    return value;
            }

            return fallback;
        }

        private static decimal GetDecimal(JsonElement element, params string[] names)
        {
            foreach (var name in names)
            {
                if (!TryGetProperty(element, name, out var property))
                    continue;

                if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var value))
                    return value;

                if (property.ValueKind == JsonValueKind.String && decimal.TryParse(property.GetString(), out value))
                    return value;
            }

            return 0;
        }

        private static bool TryGetProperty(JsonElement element, string name, out JsonElement property)
        {
            foreach (var item in element.EnumerateObject())
            {
                if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    property = item.Value;
                    return true;
                }
            }

            property = default;
            return false;
        }
    }
}
