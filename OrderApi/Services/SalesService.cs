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
        private readonly ILogger<SalesService> _logger;

        public SalesService(
            OrderDbContext dbContext,
            IOrderService orderService,
            IPaymentService paymentService,
            IOutboxService outboxService,
            ILogger<SalesService> logger)
        {
            _dbContext = dbContext;
            _orderService = orderService;
            _paymentService = paymentService;
            _outboxService = outboxService;
            _logger = logger;
        }

        public async Task<CalculateTotalResponseDto> CalculateTotalAsync(CalculateTotalDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new InvalidOperationException("Cần ít nhất một sản phẩm.");

            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var unitPrice = item.UnitPrice;
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

        public async Task<CheckoutResponseDto> ApplyDiscountAsync(ApplyDiscountDto dto)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Debt)
                .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId)
                ?? throw new KeyNotFoundException($"Order {dto.OrderId} not found");

            if (dto.DiscountAmount < 0)
                throw new InvalidOperationException("Chiết khấu không được nhỏ hơn 0.");

            if (dto.DiscountAmount > order.TotalAmount)
                throw new InvalidOperationException("Chiết khấu không được lớn hơn tổng tiền.");

            var oldDebtAmount = order.DebtAmount;
            order.DiscountAmount = dto.DiscountAmount;
            order.FinalAmount = order.TotalAmount - dto.DiscountAmount;
            order.DebtAmount = Math.Max(order.FinalAmount - order.PaidAmount, 0);

            if (order.DebtAmount == 0)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.OrderStatus = OrderStatus.Paid;
            }
            else
            {
                order.PaymentStatus = order.PaidAmount > 0 ? PaymentStatus.Partial : PaymentStatus.Unpaid;
                order.OrderStatus = OrderStatus.Debt;
            }

            if (order.Debt != null)
            {
                order.Debt.DebtAmount = order.DebtAmount;
                order.Debt.PaidAmount = Math.Min(order.Debt.PaidAmount, order.Debt.DebtAmount);
                order.Debt.DebtStatus = order.DebtAmount == 0
                    ? DebtStatus.Paid
                    : (order.Debt.PaidAmount > 0 ? DebtStatus.Partial : DebtStatus.Unpaid);
                order.Debt.UpdatedAt = DateTime.UtcNow;
            }

            if (order.Customer != null)
            {
                order.Customer.CurrentDebt = Math.Max(order.Customer.CurrentDebt + order.DebtAmount - oldDebtAmount, 0);
                order.Customer.UpdatedAt = DateTime.UtcNow;
            }

            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return new CheckoutResponseDto
            {
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                PaidAmount = order.PaidAmount,
                DebtAmount = order.DebtAmount,
                PaymentStatus = order.PaymentStatus.ToString(),
                OrderStatus = order.OrderStatus.ToString()
            };
        }

        public async Task<CheckoutResponseDto> CheckoutAsync(CheckoutDto dto, string createdBy)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new InvalidOperationException("Một đơn hàng phải có ít nhất một sản phẩm.");

            var customer = await _dbContext.Customers.FindAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException($"Customer {dto.CustomerId} not found");

            var items = new List<CreateOrderDetailDto>();
            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new InvalidOperationException("Số lượng sản phẩm phải lớn hơn 0.");

                var stock = await _dbContext.ProductStockCaches
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId)
                    ?? throw new InvalidOperationException($"Product {item.ProductId} stock info not found");

                if (stock.QuantityAvailable < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");

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
            var orderCode = $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant()}";
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
