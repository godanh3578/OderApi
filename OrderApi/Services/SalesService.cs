using OrderApi.Data;
using OrderApi.DTOs.Sales;
using OrderApi.DTOs.Orders;
using OrderApi.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderApi.Services
{
    public class SalesService : ISalesService
    {
        private readonly OrderDbContext _dbContext;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IOutboxService _outboxService;
        private readonly IProductCatalogClient _productCatalogClient;
        private readonly ILogger<SalesService> _logger;

        public SalesService(
            OrderDbContext dbContext,
            IOrderService orderService,
            IPaymentService paymentService,
            IOutboxService outboxService,
            IProductCatalogClient productCatalogClient,
            ILogger<SalesService> logger)
        {
            _dbContext = dbContext;
            _orderService = orderService;
            _paymentService = paymentService;
            _outboxService = outboxService;
            _productCatalogClient = productCatalogClient;
            _logger = logger;
        }

        public async Task<CalculateTotalResponseDto> CalculateTotalAsync(CalculateTotalDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new InvalidOperationException("Cần ít nhất một sản phẩm.");

            if (dto.DiscountAmount < 0)
                throw new InvalidOperationException("Chiết khấu không được nhỏ hơn 0.");

            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var unitPrice = item.UnitPrice;
                if (unitPrice <= 0)
                {
                    var product = await _productCatalogClient.GetProductAsync(item.ProductId)
                        ?? throw new InvalidOperationException($"Product {item.ProductId} price/stock info not found");
                    unitPrice = product.SellingPrice;
                }

                if (unitPrice <= 0)
                {
                    var stock = await _dbContext.ProductStockCaches
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId)
                        ?? throw new InvalidOperationException($"Không có giá/tồn kho cho sản phẩm {item.ProductId}.");
                    unitPrice = stock.SellingPrice;
                }

                totalAmount += item.Quantity * unitPrice;
            }

            if (dto.DiscountAmount > totalAmount)
                throw new InvalidOperationException("Chiết khấu không được lớn hơn tổng tiền.");

            return new CalculateTotalResponseDto
            {
                TotalAmount = totalAmount,
                DiscountAmount = dto.DiscountAmount,
                FinalAmount = totalAmount - dto.DiscountAmount
            };
        }

        public async Task<CheckoutResponseDto> CheckoutAsync(CheckoutDto dto, string createdBy)
        {
            if (!string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            {
                var existingOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.IdempotencyKey == dto.IdempotencyKey);
                if (existingOrder != null)
                {
                    return new CheckoutResponseDto
                    {
                        OrderId = existingOrder.OrderId,
                        OrderCode = existingOrder.OrderCode,
                        TotalAmount = existingOrder.TotalAmount,
                        DiscountAmount = existingOrder.DiscountAmount,
                        FinalAmount = existingOrder.FinalAmount,
                        PaidAmount = existingOrder.PaidAmount,
                        DebtAmount = existingOrder.DebtAmount,
                        PaymentStatus = existingOrder.PaymentStatus.ToString(),
                        OrderStatus = existingOrder.OrderStatus.ToString()
                    };
                }
            }

            if (dto.Items == null || dto.Items.Count == 0)
                throw new InvalidOperationException("Một đơn hàng phải có ít nhất một sản phẩm.");

            if (dto.DiscountAmount < 0)
                throw new InvalidOperationException("Chiết khấu không được nhỏ hơn 0.");

            if (dto.PaidAmount < 0)
                throw new InvalidOperationException("Số tiền thanh toán không được nhỏ hơn 0.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var customer = await _dbContext.Customers.FindAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException($"Customer {dto.CustomerId} not found");

            var items = new List<CreateOrderDetailDto>();
            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new InvalidOperationException("Số lượng sản phẩm phải lớn hơn 0.");

                var catalogProduct = await _productCatalogClient.GetProductAsync(item.ProductId)
                    ?? throw new InvalidOperationException($"Product {item.ProductId} stock info not found");

                if (catalogProduct.QuantityAvailable < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");

                var cachedStock = await _dbContext.ProductStockCaches
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (cachedStock == null)
                {
                    cachedStock = new ProductStockCache
                    {
                        ProductId = catalogProduct.ProductId
                    };
                    _dbContext.ProductStockCaches.Add(cachedStock);
                }

                cachedStock.ProductCode = string.IsNullOrWhiteSpace(catalogProduct.ProductCode)
                    ? $"SP{catalogProduct.ProductId:D3}"
                    : catalogProduct.ProductCode;
                cachedStock.ProductName = string.IsNullOrWhiteSpace(catalogProduct.ProductName)
                    ? $"Product {catalogProduct.ProductId}"
                    : catalogProduct.ProductName;
                cachedStock.CategoryName = catalogProduct.CategoryName;
                cachedStock.SellingPrice = catalogProduct.SellingPrice;
                cachedStock.QuantityAvailable = catalogProduct.QuantityAvailable;
                cachedStock.IsDeleted = false;

                var stock = await _dbContext.ProductStockCaches
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId)
                    ?? throw new InvalidOperationException($"Product {item.ProductId} stock info not found");

                if (stock.QuantityAvailable < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");

                stock.QuantityAvailable -= item.Quantity;
                if (stock.QuantityAvailable <= 0)
                {
                    stock.QuantityAvailable = 0;
                    stock.StockStatus = StockStatus.OutOfStock;
                }
                else if (stock.QuantityAvailable <= 5)
                {
                    stock.StockStatus = StockStatus.LowStock;
                }
                else
                {
                    stock.StockStatus = StockStatus.InStock;
                }

                stock.LastUpdatedAt = DateTime.UtcNow;

                items.Add(new CreateOrderDetailDto
                {
                    ProductId = item.ProductId,
                    ProductCode = stock.ProductCode,
                    ProductName = stock.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = stock.SellingPrice,
                    DiscountAmount = 0
                });
            }

            var createOrderDto = new CreateOrderDto
            {
                IdempotencyKey = dto.IdempotencyKey,
                CustomerId = dto.CustomerId,
                CustomerName = customer.FullName,
                Items = items,
                DiscountAmount = dto.DiscountAmount,
                CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "sales01" : createdBy
            };

            // Create without duplicate outbox — we'll enqueue after payment/debt updates
            var orderEntity = await CreateOrderWithoutOutboxAsync(createOrderDto);

            if (dto.PaidAmount > 0)
            {
                if (dto.PaidAmount > orderEntity.FinalAmount - orderEntity.PaidAmount)
                    throw new InvalidOperationException("Số tiền thanh toán không được vượt quá số tiền còn lại.");

                await _paymentService.RecordPaymentAsync(orderEntity.OrderId, new DTOs.Payments.CreatePaymentDto
                {
                    OrderId = orderEntity.OrderId,
                    PaymentMethod = dto.PaymentMethod,
                    Amount = dto.PaidAmount,
                    Note = "Payment at checkout"
                });

                orderEntity = await _dbContext.Orders.FindAsync(orderEntity.OrderId)
                    ?? orderEntity;
            }

            if (orderEntity.PaidAmount < orderEntity.FinalAmount)
            {
                var debtAmount = orderEntity.FinalAmount - orderEntity.PaidAmount;
                var debt = new Debt
                {
                    CustomerId = dto.CustomerId,
                    OrderId = orderEntity.OrderId,
                    DebtAmount = debtAmount,
                    DebtStatus = orderEntity.PaidAmount > 0 ? DebtStatus.Partial : DebtStatus.Unpaid,
                    DueDate = DateTime.UtcNow.AddDays(30)
                };
                _dbContext.Debts.Add(debt);

                orderEntity.DebtAmount = debtAmount;
                orderEntity.OrderStatus = OrderStatus.Debt;
                orderEntity.PaymentStatus = orderEntity.PaidAmount > 0 ? PaymentStatus.Partial : PaymentStatus.Unpaid;

                customer.CurrentDebt += debtAmount;
            }
            else
            {
                orderEntity.OrderStatus = OrderStatus.Paid;
                orderEntity.PaymentStatus = PaymentStatus.Paid;
                orderEntity.DebtAmount = 0;
            }

            customer.TotalSpent += orderEntity.FinalAmount;
            customer.UpdatedAt = DateTime.UtcNow;
            orderEntity.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await _outboxService.EnqueueOrderCreatedAsync(orderEntity.OrderId);

            await transaction.CommitAsync();

            _logger.LogInformation("Checkout completed: {OrderCode}", orderEntity.OrderCode);

            return new CheckoutResponseDto
            {
                OrderId = orderEntity.OrderId,
                OrderCode = orderEntity.OrderCode,
                TotalAmount = orderEntity.TotalAmount,
                DiscountAmount = orderEntity.DiscountAmount,
                FinalAmount = orderEntity.FinalAmount,
                PaidAmount = orderEntity.PaidAmount,
                DebtAmount = orderEntity.DebtAmount,
                PaymentStatus = orderEntity.PaymentStatus.ToString(),
                OrderStatus = orderEntity.OrderStatus.ToString()
            };
        }

        private async Task<Order> CreateOrderWithoutOutboxAsync(CreateOrderDto dto)
        {
            var orderCode = $"ORD{(await _dbContext.Orders.IgnoreQueryFilters().CountAsync() + 1):D6}";
            var order = new Order
            {
                OrderCode = orderCode,
                IdempotencyKey = dto.IdempotencyKey,
                CustomerId = dto.CustomerId,
                CreatedBy = dto.CreatedBy,
                OrderDate = DateTime.UtcNow,
                DiscountAmount = dto.DiscountAmount,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid,
            };

            decimal totalAmount = 0;
            foreach (var itemDto in dto.Items)
            {
                var detail = new OrderDetail
                {
                    ProductId = itemDto.ProductId,
                    ProductCode = itemDto.ProductCode,
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    DiscountAmount = itemDto.DiscountAmount,
                    SubTotal = (itemDto.Quantity * itemDto.UnitPrice) - itemDto.DiscountAmount
                };
                order.Items.Add(detail);
                totalAmount += detail.SubTotal;
            }

            order.TotalAmount = totalAmount;
            order.FinalAmount = totalAmount - dto.DiscountAmount;

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();
            return order;
        }
    }
}
