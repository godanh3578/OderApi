using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.DTOs.Customers;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize(Roles = "Admin,Sales")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly OrderDbContext _context;

        public CustomersController(ICustomerService customerService, OrderDbContext context)
        {
            _customerService = customerService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var customers = await _customerService.GetAllCustomersAsync(search, page, pageSize);
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpGet("{id}/purchase-history")]
        public async Task<IActionResult> GetPurchaseHistory(int id)
        {
            try
            {
                var history = await _customerService.GetPurchaseHistoryAsync(id);
                return Ok(history);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/debts")]
        public async Task<IActionResult> GetDebts(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            var debts = await _context.Debts
                .Where(d => d.CustomerId == id)
                .ToListAsync();

            return Ok(new
            {
                customerId = id,
                customerName = customer.FullName,
                currentDebt = customer.CurrentDebt,
                debts
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(dto);
                return Ok(customer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
        {
            try
            {
                var customer = await _customerService.UpdateCustomerAsync(id, dto);
                return Ok(customer);
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
            var ok = await _customerService.DeleteCustomerAsync(id);
            if (!ok) return NotFound();
            return Ok();
        }
    }
}
