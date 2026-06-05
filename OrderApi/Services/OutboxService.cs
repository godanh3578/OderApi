using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Events;
using OrderApi.Models;

namespace OrderApi.Services
{
    public class OutboxService : IOutboxService
    {
        private readonly OrderDbContext _dbContext;

        public OutboxService(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task EnqueueAsync<T>(string eventName, T payload, CancellationToken cancellationToken = default)
        {
            var message = new OutboxMessage
            {
                EventName = eventName,
                Payload = JsonSerializer.Serialize(payload),
                Status = OutboxMessageStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.OutboxMessages.Add(message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task EnqueueOrderCreatedAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

            if (order == null)
                return;

            var payment = order.Payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();

            var evt = new OrderCreatedEvent
            {
                EventName = "order.created",
                OrderId = order.OrderId,
                OrderCode = order.OrderCode,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? "",
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                PaidAmount = order.PaidAmount,
                DebtAmount = order.DebtAmount,
                PaymentMethod = payment?.PaymentMethod.ToString() ?? "",
                PaymentStatus = order.PaymentStatus.ToString(),
                OrderStatus = order.OrderStatus.ToString(),
                CreatedBy = order.CreatedBy,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderCreatedEventItem
                {
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    SubTotal = i.SubTotal
                }).ToList()
            };

            await EnqueueAsync("order.created", evt, cancellationToken);
        }
    }
}
