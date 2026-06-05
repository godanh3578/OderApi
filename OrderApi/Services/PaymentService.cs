using OrderApi.Data;
using OrderApi.DTOs.Payments;
using OrderApi.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly OrderDbContext _dbContext;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(OrderDbContext dbContext, ILogger<PaymentService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _dbContext.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _dbContext.Payments.FindAsync(paymentId);
            if (payment == null)
                return null;

            return MapToDto(payment);
        }

        public async Task<List<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var payments = await _dbContext.Payments
                .Where(p => p.OrderId == orderId)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        public async Task<PaymentDto> RecordPaymentAsync(int orderId, CreatePaymentDto dto)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found");

            var paymentCode = $"PAY{DateTime.Now:yyyyMMddHHmmss}";

            var payment = new Payment
            {
                OrderId = orderId,
                PaymentCode = paymentCode,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
                Note = dto.Note,
            };

            if (Enum.TryParse<PaymentMethod>(dto.PaymentMethod, out var method))
                payment.PaymentMethod = method;

            if (dto.Amount < 0)
                throw new InvalidOperationException("Số tiền thanh toán không được nhỏ hơn 0.");

            var remaining = order.FinalAmount - order.PaidAmount;
            if (dto.Amount > remaining)
                throw new InvalidOperationException("Số tiền thanh toán không được vượt quá số tiền còn lại.");

            order.PaidAmount += dto.Amount;

            if (order.PaidAmount >= order.FinalAmount)
            {
                payment.PaymentStatus = PaymentStatus.Paid;
                order.PaymentStatus = PaymentStatus.Paid;
                order.OrderStatus = OrderStatus.Paid;
                order.DebtAmount = 0;
            }
            else if (dto.Amount > 0)
            {
                payment.PaymentStatus = PaymentStatus.Partial;
                order.PaymentStatus = PaymentStatus.Partial;
                order.OrderStatus = OrderStatus.Debt;
                order.DebtAmount = order.FinalAmount - order.PaidAmount;
            }

            order.UpdatedAt = DateTime.UtcNow;

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Payment recorded: {paymentCode} for order {orderId}");

            return MapToDto(payment);
        }

        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                PaymentCode = payment.PaymentCode,
                PaymentMethod = payment.PaymentMethod.ToString(),
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentStatus = payment.PaymentStatus.ToString(),
                Note = payment.Note,
                CreatedAt = payment.CreatedAt
            };
        }
    }
}
