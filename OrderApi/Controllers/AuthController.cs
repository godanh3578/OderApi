using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] DemoLoginRequest request)
        {
            var role = string.IsNullOrWhiteSpace(request.Role) ? "Sales" : request.Role;
            var username = string.IsNullOrWhiteSpace(request.Username) ? "sales01" : request.Username;

            var key = _config["Jwt:Key"] ?? "OrderApiSuperSecretKey123!@#ChangeMe2026";
            var issuer = _config["Jwt:Issuer"] ?? "OrderApi";
            var audience = _config["Jwt:Audience"] ?? "OrderApiUsers";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("role", role)
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                username,
                role
            });
        }
    }

    public class DemoLoginRequest
    {
        public string Username { get; set; } = "sales01";
        public string Role { get; set; } = "Sales";
    }
}
