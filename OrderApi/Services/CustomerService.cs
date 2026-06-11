using OrderApi.Data;
using OrderApi.DTOs.Customers;
using OrderApi.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderApi.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly OrderDbContext _dbContext;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(OrderDbContext dbContext, ILogger<CustomerService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                return null;

            return MapToDto(customer);
        }

        public async Task<CustomerDto?> GetCustomerByCodeAsync(string customerCode)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerCode == customerCode);

            if (customer == null)
                return null;

            return MapToDto(customer);
        }

        public async Task<CustomerDto?> GetCustomerByPhoneAsync(string phone)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Phone == phone);

            if (customer == null)
                return null;

            return MapToDto(customer);
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _dbContext.Customers
                .Include(c => c.Orders)
                .ToListAsync();

            return customers.Select(MapToDto).ToList();
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerCode))
            {
                var count = await _dbContext.Customers.IgnoreQueryFilters().CountAsync();
                dto.CustomerCode = $"KH{(count + 1):D6}";
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var phoneExists = await _dbContext.Customers
                    .AnyAsync(c => c.Phone == dto.Phone.Trim());
                if (phoneExists)
                    throw new InvalidOperationException("Số điện thoại đã được đăng ký.");
            }

            var existing = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.CustomerCode == dto.CustomerCode);

            if (existing != null)
                throw new InvalidOperationException($"Customer code {dto.CustomerCode} already exists");

            var customer = new Customer
            {
                CustomerCode = dto.CustomerCode,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                Status = CustomerStatus.Active
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Customer created: {dto.CustomerCode}");

            return MapToDto(customer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto)
        {
            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer {customerId} not found");

            customer.FullName = dto.FullName;
            customer.Phone = dto.Phone;
            customer.Email = dto.Email;
            customer.Address = dto.Address;

            if (Enum.TryParse<CustomerStatus>(dto.Status, out var status))
                customer.Status = status;

            customer.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Customer {customerId} updated");

            return MapToDto(customer);
        }

        public async Task<CustomerDto> UpdateCustomerProfileAsync(int customerId, UpdateCustomerProfileDto dto)
        {
            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer {customerId} not found");

            var newPhone = (dto.Phone ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(newPhone))
                throw new InvalidOperationException("Số điện thoại không được để trống.");

            if (!string.Equals(customer.Phone, newPhone, StringComparison.Ordinal))
            {
                var phoneExists = await _dbContext.Customers
                    .AnyAsync(c => c.CustomerId != customerId && c.Phone == newPhone);

                if (phoneExists)
                    throw new InvalidOperationException("Số điện thoại đã được đăng ký.");
            }

            customer.FullName = dto.FullName;
            customer.Phone = newPhone;
            customer.Email = dto.Email;
            customer.Address = dto.Address;
            customer.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer == null)
                return false;

            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Customer {customerId} deleted");
            return true;
        }

        public async Task<CustomerPurchaseHistoryDto> GetPurchaseHistoryAsync(int customerId)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException($"Customer {customerId} not found");

            var orders = customer.Orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new PurchaseHistoryItemDto
                {
                    OrderCode = o.OrderCode,
                    OrderDate = o.OrderDate,
                    FinalAmount = o.FinalAmount,
                    PaymentStatus = o.PaymentStatus.ToString()
                }).ToList();

            return new CustomerPurchaseHistoryDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.FullName,
                TotalSpent = customer.TotalSpent,
                CurrentDebt = customer.CurrentDebt,
                Orders = orders
            };
        }

        private CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                TotalSpent = customer.TotalSpent,
                CurrentDebt = customer.CurrentDebt,
                Status = customer.Status.ToString(),
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }
}
