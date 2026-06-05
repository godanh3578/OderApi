using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.DTOs.Suppliers;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    [Authorize(Roles = "Admin,Sales")]
    public class SuppliersController : ControllerBase
    {
        private readonly OrderDbContext _context;

        public SuppliersController(OrderDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _context.Suppliers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s =>
                    s.SupplierName.Contains(term) ||
                    s.SupplierCode.Contains(term) ||
                    s.ContactPerson.Contains(term));
            }

            var suppliers = await query.OrderBy(s => s.SupplierName).ToListAsync();
            return Ok(suppliers.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            return Ok(MapToDto(supplier));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
        {
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

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return Ok(MapToDto(supplier));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            supplier.SupplierName = dto.SupplierName;
            supplier.ContactPerson = dto.ContactPerson;
            supplier.Phone = dto.Phone;
            supplier.Email = dto.Email;
            supplier.Address = dto.Address;

            if (Enum.TryParse<SupplierStatus>(dto.Status, true, out var status))
                supplier.Status = status;

            supplier.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(MapToDto(supplier));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return Ok();
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
