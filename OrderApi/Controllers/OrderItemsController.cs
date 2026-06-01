using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/orders/{orderId}/items")]
    public class OrderItemsController : ControllerBase
    {
        private readonly OrderDbContext _context;

        public OrderItemsController(OrderDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Đơn hàng không tồn tại.");

            var items = await _context.OrderItems
                .Where(i => i.OrderId == orderId)
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int orderId, OrderItem item)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound("Đơn hàng không tồn tại.");

            item.OrderId = orderId;
            _context.OrderItems.Add(item);

            // Tính lại TotalAmount
            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.Price)
                                + item.Quantity * item.Price;
            order.TotalAmount *= (1 - order.Discount / 100);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(item);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi thêm sản phẩm vào đơn hàng.");
            }
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteItem(int orderId, int itemId)
        {
            var item = await _context.OrderItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OrderId == orderId);

            if (item == null) return NotFound("Không tìm thấy sản phẩm trong đơn hàng.");

            _context.OrderItems.Remove(item);

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                order.TotalAmount = order.Items
                    .Where(i => i.Id != itemId)
                    .Sum(i => i.Quantity * i.Price)
                    * (1 - order.Discount / 100);
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi xóa sản phẩm.");
            }
        }
    }
}