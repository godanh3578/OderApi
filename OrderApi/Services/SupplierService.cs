using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.DTOs.Suppliers;
using OrderApi.Models;

namespace OrderApi.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly OrderDbContext _dbContext;

        public SupplierService(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SupplierDto>> GetAllSuppliersAsync(string? search = null, int page = 1, int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _dbContext.Suppliers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s =>
                    s.SupplierName.Contains(term) ||
                    s.SupplierCode.Contains(term) ||
                    s.ContactPerson.Contains(term));
            }

            var suppliers = await query
                .OrderBy(s => s.SupplierName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return suppliers.Select(MapToDto).ToList();
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(int supplierId)
        {
            var supplier = await _dbContext.Suppliers.FindAsync(supplierId);
            return supplier == null ? null : MapToDto(supplier);
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto)
        {
            var existing = await _dbContext.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierCode == dto.SupplierCode);

            if (existing != null)
                throw new InvalidOperationException($"Supplier code {dto.SupplierCode} already exists");

            var supplier = new Supplier
            {
                SupplierCode = dto.SupplierCode,
                SupplierName = dto.SupplierName,
                ContactPerson = dto.ContactPerson,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                Status = SupplierStatus.Active
            };

            _dbContext.Suppliers.Add(supplier);
            await _dbContext.SaveChangesAsync();
            return MapToDto(supplier);
        }

        public async Task<SupplierDto> UpdateSupplierAsync(int supplierId, UpdateSupplierDto dto)
        {
            var supplier = await _dbContext.Suppliers.FindAsync(supplierId)
                ?? throw new KeyNotFoundException($"Supplier {supplierId} not found");

            supplier.SupplierName = dto.SupplierName;
            supplier.ContactPerson = dto.ContactPerson;
            supplier.Phone = dto.Phone;
            supplier.Email = dto.Email;
            supplier.Address = dto.Address;

            if (Enum.TryParse<SupplierStatus>(dto.Status, true, out var status))
                supplier.Status = status;

            supplier.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return MapToDto(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(int supplierId)
        {
            var supplier = await _dbContext.Suppliers.FindAsync(supplierId);
            if (supplier == null)
                return false;

            _dbContext.Suppliers.Remove(supplier);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private static SupplierDto MapToDto(Supplier supplier) => new()
        {
            SupplierId = supplier.SupplierId,
            SupplierCode = supplier.SupplierCode,
            SupplierName = supplier.SupplierName,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            Status = supplier.Status.ToString(),
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt
        };
    }
}
