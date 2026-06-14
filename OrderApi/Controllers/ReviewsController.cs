using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/Reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly OrderDbContext _context;

        public ReviewsController(OrderDbContext context)
        {
            _context = context;
        }

        [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var exists = await _context.Reviews
        .AnyAsync(r => r.CustomerId == dto.CustomerId && r.ProductId == dto.ProductId);
     if (exists) return BadRequest(new { message = "Bạn đã đánh giá sản phẩm này." });

        var review = new Review
        {
        CustomerId = dto.CustomerId,
        ProductId = dto.ProductId,
        Rating = dto.Rating,
        Comment = dto.Comment ?? ""
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return Ok(review);
    }

      [HttpGet]
[AllowAnonymous]
public async Task<IActionResult> GetByProduct([FromQuery] int productId)
{
    var reviews = await _context.Reviews
        .Where(r => r.ProductId == productId)
        .Include(r => r.Customer)
        .Select(r => new {
            r.ReviewId,
            r.Rating,
            r.Comment,
            r.CreatedAt,
            customerName = r.Customer.FullName
        })
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    var averageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;

    return Ok(new {
        averageRating = Math.Round(averageRating, 1),
        totalReviews = reviews.Count,
        reviews
    });
    }
    }
}