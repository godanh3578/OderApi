using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/stock-cache")]
    [Authorize(Roles = "Admin,Sales,Warehouse")]
    public class StockCacheController : ControllerBase
    {
        private readonly OrderDbContext _context;

        public StockCacheController(OrderDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.ProductStockCaches
                .OrderBy(p => p.ProductName)
                .ToListAsync();
            return Ok(items);
        }
    }
}
