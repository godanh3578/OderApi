using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderApi.Data;
using OrderApi.Models;
using OrderApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var localDbPath = Path.Combine(builder.Environment.ContentRootPath, ".localdb");
Directory.CreateDirectory(localDbPath);
var dataProtectionPath = Path.Combine(localDbPath, "keys");
Directory.CreateDirectory(dataProtectionPath);
AppDomain.CurrentDomain.SetData("DataDirectory", localDbPath);
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?.Replace("|DataDirectory|", localDbPath);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("OrderApi");

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(defaultConnection));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "OrderApiSuperSecretKey123!@#ChangeMe2026";
var jwtIss = builder.Configuration["Jwt:Issuer"] ?? "OrderApi";
var jwtAud = builder.Configuration["Jwt:Audience"] ?? "OrderApiUsers";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIss,
            ValidAudience = jwtAud,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            RoleClaimType = "role"
        };
    });

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDebtService, DebtService>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var gatewayBaseUrl = config["ProductIntegration:GatewayBaseUrl"];
    if (!string.IsNullOrWhiteSpace(gatewayBaseUrl))
        client.BaseAddress = new Uri(gatewayBaseUrl);

    client.Timeout = TimeSpan.FromSeconds(config.GetValue("ProductIntegration:TimeoutSeconds", 3));
});

builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<StockConsumerService>();
builder.Services.AddHostedService<OutboxDispatcherService>();
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            if (builder.Environment.IsDevelopment())
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            else
                policy.SetIsOriginAllowed(_ => false).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
    await db.Database.ExecuteSqlRawAsync("""
        IF OBJECT_ID(N'[ProductStockCaches]', N'U') IS NOT NULL
           AND COL_LENGTH(N'ProductStockCaches', N'CategoryName') IS NULL
        BEGIN
            ALTER TABLE [ProductStockCaches]
            ADD [CategoryName] nvarchar(100) NOT NULL
                CONSTRAINT [DF_ProductStockCaches_CategoryName] DEFAULT N''
        END
        """);
    await DbSeeder.SeedAsync(db);
    await RestoreCancelledOrderStockAsync(db);
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Migration failed: {ex.Message}");
    Console.WriteLine("Database might not be available. API will run in demo mode.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static async Task RestoreCancelledOrderStockAsync(OrderDbContext db)
{
    var cancelledOrders = await db.Orders
        .IgnoreQueryFilters()
        .Include(o => o.Items)
        .Where(o => o.OrderStatus == OrderStatus.Cancelled && o.StockRestoredAt == null)
        .ToListAsync();

    if (cancelledOrders.Count == 0)
        return;

    foreach (var order in cancelledOrders)
    {
        foreach (var item in order.Items)
        {
            var stock = await db.ProductStockCaches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

            if (stock == null)
            {
                stock = new ProductStockCache
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    SellingPrice = item.UnitPrice
                };
                db.ProductStockCaches.Add(stock);
            }

            stock.QuantityAvailable += item.Quantity;
            stock.StockStatus = stock.QuantityAvailable <= 0
                ? StockStatus.OutOfStock
                : stock.QuantityAvailable <= 5
                    ? StockStatus.LowStock
                    : StockStatus.InStock;
            stock.IsDeleted = false;
            stock.LastUpdatedAt = DateTime.UtcNow;
        }

        order.StockRestoredAt = DateTime.UtcNow;
    }

    await db.SaveChangesAsync();
    Console.WriteLine($"Restored stock for {cancelledOrders.Count} cancelled order(s).");
}
