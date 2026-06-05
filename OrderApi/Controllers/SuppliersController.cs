using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.DTOs.Suppliers;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    [Authorize(Roles = "Admin,Sales")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync(search, page, pageSize);
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (supplier == null) return NotFound();
            return Ok(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
        {
            try
            {
                var supplier = await _supplierService.CreateSupplierAsync(dto);
                return Ok(supplier);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
        {
            try
            {
                var supplier = await _supplierService.UpdateSupplierAsync(id, dto);
                return Ok(supplier);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _supplierService.DeleteSupplierAsync(id);
            if (!ok) return NotFound();
            return Ok();
        }
    }
}
