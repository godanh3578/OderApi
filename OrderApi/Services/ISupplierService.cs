using OrderApi.DTOs.Suppliers;

namespace OrderApi.Services
{
    public interface ISupplierService
    {
        Task<List<SupplierDto>> GetAllSuppliersAsync(string? search = null, int page = 1, int pageSize = 20);
        Task<SupplierDto?> GetSupplierByIdAsync(int supplierId);
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto);
        Task<SupplierDto> UpdateSupplierAsync(int supplierId, UpdateSupplierDto dto);
        Task<bool> DeleteSupplierAsync(int supplierId);
    }
}
