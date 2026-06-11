using OrderApi.Data;
using OrderApi.DTOs.Orders;
using OrderApi.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderDbContext _dbContext;
        private readonly IOutboxService _outboxService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            OrderDbContext dbContext,
            IOutboxService outboxService,
            ILogger<OrderService> logger)
        {
            _dbContext = dbContext;
            _outboxService = outboxService;
            _logger = logger;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            return order == null ? null : MapToDto(order);
        }

        public async Task<OrderDto?> GetOrderByCodeAsync(string orderCode)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            return order == null ? null : MapToDto(order);
        }

        public async Task<OrderDto?> LookupOrderAsync(string orderCode, string phone)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode.Trim());

            if (order?.Customer == null || order.Customer.Phone != phone.Trim())
                return null;

            return MapToDto(order);
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync(string? search = null)
        {
            return await QueryOrdersAsync(search, null);
        }

        public async Task<List<OrderDto>> GetOrdersByCustomerIdAsync(int customerId, string? search = null)
        {
            return await QueryOrdersAsync(search, customerId);
        }

        private async Task<List<OrderDto>> QueryOrdersAsync(string? search, int? customerId)
        {
            var query = _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .AsQueryable();

            if (customerId.HasValue)
                query = query.Where(o => o.CustomerId == customerId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(o =>
                    o.OrderCode.Contains(term) ||
                    (o.Customer != null && o.Customer.FullName.Contains(term)) ||
                    o.OrderDate.ToString().Contains(term));
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new InvalidOperationException("Một đơn hàng phải có ít nhất một sản phẩm.");

            if (string.IsNullOrWhiteSpace(dto.CreatedBy))
                throw new InvalidOperationException("Đơn hàng phải có nhân viên tạo đơn.");

            if (dto.DiscountAmount < 0)
                throw new InvalidOperationException("Chiết khấu không được nhỏ hơn 0.");

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Quantity <= 0)
                    throw new InvalidOperationException("Số lượng sản phẩm phải lớn hơn 0.");

                if (itemDto.DiscountAmount < 0)
                    throw new InvalidOperationException("Chiết khấu sản phẩm không được nhỏ hơn 0.");

                var stock = await _dbContext.ProductStockCaches
                    .FirstOrDefaultAsync(p => p.ProductId == itemDto.ProductId);

                if (stock == null)
                    throw new InvalidOperationException($"Không có dữ liệu tồn kho cho sản phẩm {itemDto.ProductId}.");

                if (stock.QuantityAvailable < itemDto.Quantity)
                    throw new InvalidOperationException($"Sản phẩm {itemDto.ProductId} không đủ tồn kho.");
            }

            var orderCode = await GenerateOrderCodeAsync();
            var order = new Order
            {
                OrderCode = orderCode,
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
                var stock = await _dbContext.ProductStockCaches
                    .FirstAsync(p => p.ProductId == itemDto.ProductId);

                var unitPrice = itemDto.UnitPrice > 0 ? itemDto.UnitPrice : stock.SellingPrice;
                var detail = new OrderDetail
                {
                    ProductId = itemDto.ProductId,
                    ProductCode = string.IsNullOrEmpty(itemDto.ProductCode) ? stock.ProductCode : itemDto.ProductCode,
                    ProductName = string.IsNullOrEmpty(itemDto.ProductName) ? stock.ProductName : itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = unitPrice,
                    DiscountAmount = itemDto.DiscountAmount,
                    SubTotal = (itemDto.Quantity * unitPrice) - itemDto.DiscountAmount
                };
                order.Items.Add(detail);
                totalAmount += detail.SubTotal;
            }

            if (dto.DiscountAmount > totalAmount)
                throw new InvalidOperationException("Chiết khấu không được lớn hơn tổng tiền.");

            order.TotalAmount = totalAmount;
            order.FinalAmount = totalAmount - dto.DiscountAmount;

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            await _outboxService.EnqueueOrderCreatedAsync(order.OrderId);

            _logger.LogInformation("Order created: {OrderCode}", orderCode);
            return MapToDto(order);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} not found");

            if (order.OrderStatus == OrderStatus.Paid)
                throw new InvalidOperationException("Đơn đã thanh toán, không được sửa chi tiết sản phẩm.");

            if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                order.OrderStatus = orderStatus;
                order.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return MapToDto(order);
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Debt)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
                return false;

            if (order.OrderStatus == OrderStatus.Paid)
                throw new InvalidOperationException("Không thể hủy đơn đã thanh toán đủ.");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return true;

            await RestoreStockForCancelledOrderAsync(order);

            if (order.Customer != null && order.DebtAmount > 0)
            {
                order.Customer.CurrentDebt = Math.Max(0, order.Customer.CurrentDebt - order.DebtAmount);
                order.Customer.UpdatedAt = DateTime.UtcNow;
            }

            if (order.Debt != null)
            {
                order.Debt.PaidAmount = order.Debt.DebtAmount;
                order.Debt.DebtStatus = DebtStatus.Paid;
                order.Debt.UpdatedAt = DateTime.UtcNow;
            }

            order.DebtAmount = 0;
            order.OrderStatus = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderForCustomerAsync(int orderId, string phone)
        {
            var normalizedPhone = (phone ?? "").Trim();
            if (string.IsNullOrWhiteSpace(normalizedPhone))
                return false;

            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Debt)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null || order.Customer == null)
                return false;

            if (!string.Equals(order.Customer.Phone?.Trim(), normalizedPhone, StringComparison.OrdinalIgnoreCase))
                return false;

            if (order.OrderStatus == OrderStatus.Paid)
                throw new InvalidOperationException("Không thể hủy đơn đã thanh toán đủ.");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return true;

            await RestoreStockForCancelledOrderAsync(order);

            if (order.Customer != null && order.DebtAmount > 0)
            {
                order.Customer.CurrentDebt = Math.Max(0, order.Customer.CurrentDebt - order.DebtAmount);
                order.Customer.UpdatedAt = DateTime.UtcNow;
            }

            if (order.Debt != null)
            {
                order.Debt.PaidAmount = order.Debt.DebtAmount;
                order.Debt.DebtStatus = DebtStatus.Paid;
                order.Debt.UpdatedAt = DateTime.UtcNow;
            }

            order.DebtAmount = 0;
            order.OrderStatus = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task RestoreStockForCancelledOrderAsync(Order order)
        {
            foreach (var item in order.Items)
            {
                var stock = await _dbContext.ProductStockCaches
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (stock == null)
                {
                    stock = new ProductStockCache
                    {
                        ProductId = item.ProductId,
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        SellingPrice = item.UnitPrice
                    };
                    _dbContext.ProductStockCaches.Add(stock);
                }

                stock.QuantityAvailable += item.Quantity;
                stock.StockStatus = stock.QuantityAvailable <= 0
                    ? StockStatus.OutOfStock
                    : stock.QuantityAvailable <= 5
                        ? StockStatus.LowStock
                        : StockStatus.InStock;
                stock.IsDeleted = false;
                stock.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateOrderCodeAsync()
        {
            var count = await _dbContext.Orders.CountAsync();
            return $"ORD{(count + 1):D6}";
        }

        private static OrderDto MapToDto(Order order)
        {
            var latestPayment = order.Payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();

            return new OrderDto
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                PaidAmount = order.PaidAmount,
                DebtAmount = order.DebtAmount,
                PaymentStatus = order.PaymentStatus.ToString(),
                PaymentMethod = latestPayment?.PaymentMethod.ToString(),
                OrderStatus = order.OrderStatus.ToString(),
                CreatedBy = order.CreatedBy,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items.Select(i => new OrderDetailDto
                {
                    OrderDetailId = i.OrderDetailId,
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount,
                    SubTotal = i.SubTotal
                }).ToList()
            };
        }
    }
}
