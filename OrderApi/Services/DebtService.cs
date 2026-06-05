using OrderApi.Data;
using OrderApi.DTOs.Debts;
using OrderApi.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderApi.Services
{
    public class DebtService : IDebtService
    {
        private readonly OrderDbContext _dbContext;
        private readonly ILogger<DebtService> _logger;

        public DebtService(OrderDbContext dbContext, ILogger<DebtService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<DebtDto?> GetDebtByIdAsync(int debtId)
        {
            var debt = await _dbContext.Debts.FindAsync(debtId);
            if (debt == null)
                return null;

            return MapToDto(debt);
        }

        public async Task<CustomerDebtsDto> GetDebtsByCustomerIdAsync(int customerId)
        {
            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer {customerId} not found");

            var debts = await _dbContext.Debts
                .Where(d => d.CustomerId == customerId)
                .ToListAsync();

            return new CustomerDebtsDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.FullName,
                Debts = debts.Select(MapToDto).ToList()
            };
        }

        public async Task<List<DebtDto>> GetAllDebtsAsync()
        {
            var debts = await _dbContext.Debts.ToListAsync();
            return debts.Select(MapToDto).ToList();
        }

        public async Task<DebtDto> PayDebtAsync(int debtId, CreateDebtPaymentDto dto)
        {
            var debt = await _dbContext.Debts
                .Include(d => d.Customer)
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.DebtId == debtId)
                ?? throw new KeyNotFoundException($"Debt {debtId} not found");

            if (dto.Amount <= 0)
                throw new InvalidOperationException("Số tiền trả nợ phải lớn hơn 0.");

            if (dto.Amount > debt.RemainingAmount)
                throw new InvalidOperationException("Số tiền trả nợ không được vượt quá số tiền còn lại.");

            var oldRemainingAmount = debt.RemainingAmount;
            debt.PaidAmount += dto.Amount;

            // Update status
            if (debt.PaidAmount >= debt.DebtAmount)
            {
                debt.DebtStatus = DebtStatus.Paid;
                debt.PaidAmount = debt.DebtAmount;
            }
            else if (debt.PaidAmount > 0)
            {
                debt.DebtStatus = DebtStatus.Partial;
            }

            // Check if overdue
            if (debt.DueDate.HasValue && DateTime.UtcNow > debt.DueDate && debt.DebtStatus != DebtStatus.Paid)
            {
                debt.DebtStatus = DebtStatus.Overdue;
            }

            debt.UpdatedAt = DateTime.UtcNow;

            if (debt.Order != null)
            {
                debt.Order.PaidAmount = Math.Min(debt.Order.PaidAmount + dto.Amount, debt.Order.FinalAmount);
                debt.Order.DebtAmount = debt.RemainingAmount;
                debt.Order.PaymentStatus = debt.RemainingAmount == 0 ? PaymentStatus.Paid : PaymentStatus.Partial;
                debt.Order.OrderStatus = debt.RemainingAmount == 0 ? OrderStatus.Paid : OrderStatus.Debt;
                debt.Order.UpdatedAt = DateTime.UtcNow;
            }

            if (debt.Customer != null)
            {
                debt.Customer.CurrentDebt = Math.Max(debt.Customer.CurrentDebt - (oldRemainingAmount - debt.RemainingAmount), 0);
                debt.Customer.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Debt {debtId} payment recorded: {dto.Amount}");

            return MapToDto(debt);
        }

        public async Task<DebtDto> UpdateDebtStatusAsync(int debtId, UpdateDebtStatusDto dto)
        {
            var debt = await _dbContext.Debts.FindAsync(debtId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt {debtId} not found");

            if (Enum.TryParse<DebtStatus>(dto.DebtStatus, out var status))
            {
                debt.DebtStatus = status;
                debt.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Debt {debtId} status updated to {dto.DebtStatus}");
            }

            return MapToDto(debt);
        }

        private DebtDto MapToDto(Debt debt)
        {
            return new DebtDto
            {
                DebtId = debt.DebtId,
                CustomerId = debt.CustomerId,
                OrderId = debt.OrderId,
                DebtAmount = debt.DebtAmount,
                PaidAmount = debt.PaidAmount,
                RemainingAmount = debt.RemainingAmount,
                DueDate = debt.DueDate,
                DebtStatus = debt.DebtStatus.ToString(),
                CreatedAt = debt.CreatedAt,
                UpdatedAt = debt.UpdatedAt
            };
        }
    }
}
