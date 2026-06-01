using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
// Swagger security definitions removed to avoid extra package dependency.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS from configuration: Cors:AllowedOrigins (array). In Development default to AllowAnyOrigin.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            }
            else
            {
                // No origins configured in production — block cross-origin by default.
                policy.SetIsOriginAllowed(_ => false).AllowAnyHeader().AllowAnyMethod();
            }
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// Only enable Swagger in Development by default
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCorsPolicy");

app.MapControllers();


var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
           ?? builder.Configuration["Host:Urls"]
           ?? "http://192.168.29.23:5002";

app.Run();