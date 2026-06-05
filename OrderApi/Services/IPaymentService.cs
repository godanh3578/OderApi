using OrderApi.DTOs.Payments;

namespace OrderApi.Services
{
    public interface IPaymentService
    {
        Task<PaymentDto?> GetPaymentByIdAsync(int paymentId);
        Task<List<PaymentDto>> GetAllPaymentsAsync();
        Task<List<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId);
        Task<PaymentDto> RecordPaymentAsync(int orderId, CreatePaymentDto dto);
    }
}
