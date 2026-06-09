using OrderApi.DTOs.Customers;

namespace OrderApi.Services
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetCustomerByIdAsync(int customerId);
        Task<CustomerDto?> GetCustomerByCodeAsync(string customerCode);
        Task<CustomerDto?> GetCustomerByPhoneAsync(string phone);
        Task<List<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task<CustomerDto> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto);
        Task<CustomerDto> UpdateCustomerProfileAsync(int customerId, UpdateCustomerProfileDto dto);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<CustomerPurchaseHistoryDto> GetPurchaseHistoryAsync(int customerId);
    }
}
