using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Models;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly RabbitMqPublisher _publisher;

        public OrdersController(OrderDbContext context, RabbitMqPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ToListAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            // Kiểm tra tồn kho từ cache stock.updated (N1)
            foreach (var item in order.Items)
            {
                if (StockConsumerService.StockCache.TryGetValue(item.ProductId, out var stock))
                {
                    if (stock < item.Quantity)
                        return BadRequest($"Sản phẩm ID {item.ProductId} không đủ tồn kho. Còn: {stock}");
                }
            }

            try
            {
                order.OrderDate = DateTime.UtcNow;
                order.TotalAmount = order.Items.Sum(i => i.Quantity * i.Price)
                                    * (1 - order.Discount / 100);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Publish event order.created → N3 tổng hợp báo cáo
                _publisher.Publish("order.created", new
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    TotalAmount = order.TotalAmount,
                    OrderDate = order.OrderDate,
                    Items = order.Items.Select(i => new
                    {
                        i.ProductId,
                        i.ProductName,
                        i.Quantity,
                        i.Price
                    })
                });

                return Ok(order);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi lưu đơn hàng.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi không xác định.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Order updatedOrder)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null) return NotFound();

            order.CustomerId = updatedOrder.CustomerId;
            order.Status = updatedOrder.Status;
            order.Discount = updatedOrder.Discount;
            order.Items = updatedOrder.Items;
            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.Price)
                                * (1 - order.Discount / 100);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(order);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi cập nhật đơn hàng.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi không xác định.");
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(order);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi không xác định.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            _context.Orders.Remove(order);

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi xóa đơn hàng.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi không xác định.");
            }
        }
    }
}