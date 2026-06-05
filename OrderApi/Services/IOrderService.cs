using OrderApi.DTOs.Orders;

namespace OrderApi.Services
{
    public interface IOrderService
    {
        Task<OrderDto?> GetOrderByIdAsync(int orderId);
        Task<OrderDto?> GetOrderByCodeAsync(string orderCode);
        Task<List<OrderDto>> GetAllOrdersAsync(string? search = null, int page = 1, int pageSize = 20);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CancelOrderAsync(int orderId);
        Task<bool> DeleteOrderAsync(int orderId);
    }
}
