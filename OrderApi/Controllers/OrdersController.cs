using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.DTOs.Orders;
using OrderApi.Services;
using System.Security.Claims;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/Orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? customerId)
        {
            if (customerId.HasValue)
            {
                var customerOrders = await _orderService.GetOrdersByCustomerIdAsync(customerId.Value, search);
                return Ok(customerOrders);
            }

            var user = HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return Unauthorized(new { message = "Cần đăng nhập nhân viên để xem toàn bộ đơn hàng." });

            if (!user.IsInRole("Admin") && !user.IsInRole("Sales") && !user.IsInRole("Warehouse"))
                return Forbid();

            var orders = await _orderService.GetAllOrdersAsync(search);
            return Ok(orders);
        }

        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> Lookup([FromQuery] string orderCode, [FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(orderCode) || string.IsNullOrWhiteSpace(phone))
                return BadRequest(new { message = "Vui lòng nhập mã đơn và số điện thoại." });

            var order = await _orderService.LookupOrderAsync(orderCode, phone);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng phù hợp." });

            return Ok(order);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.CreatedBy))
                    dto.CreatedBy = User.FindFirstValue(ClaimTypes.Name) ?? "sales01";

                var order = await _orderService.CreateOrderAsync(dto);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Sales,Warehouse")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var ok = await _orderService.CancelOrderAsync(id);
                if (!ok) return NotFound();
                return Ok(new { message = "Đơn hàng đã được hủy" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _orderService.DeleteOrderAsync(id);
            if (!ok) return NotFound();
            return Ok();
        }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = "";
    }
}
