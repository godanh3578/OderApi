using OrderApi.DTOs.Sales;

namespace OrderApi.Services
{
    public interface ISalesService
    {
        Task<CalculateTotalResponseDto> CalculateTotalAsync(CalculateTotalDto dto);
        Task<CheckoutResponseDto> ApplyDiscountAsync(ApplyDiscountDto dto);
        Task<CheckoutResponseDto> CheckoutAsync(CheckoutDto dto, string createdBy = "sales01");
    }
}
