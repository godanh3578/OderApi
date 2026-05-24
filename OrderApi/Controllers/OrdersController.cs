using Microsoft.AspNetCore.Mvc;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private static List<Order> orders = new List<Order>();

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var order = orders.FirstOrDefault(x => x.Id == id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpPost]
        public IActionResult Create(Order order)
        {
            orders.Add(order);
            return Ok(order);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Order updatedOrder)
        {
            var order = orders.FirstOrDefault(x => x.Id == id);

            if (order == null)
                return NotFound();

            order.CustomerName = updatedOrder.CustomerName;
            order.TotalAmount = updatedOrder.TotalAmount;
            order.Items = updatedOrder.Items;

            return Ok(order);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var order = orders.FirstOrDefault(x => x.Id == id);

            if (order == null)
                return NotFound();

            orders.Remove(order);
            return Ok();
        }
    }
}