using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Events;
using OrderApi.Models;
using RabbitMQ.Client;

namespace OrderApi.Services
{
    public class StockConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<StockConsumerService> _logger;

        public StockConsumerService(
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            ILogger<StockConsumerService> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var host = _config["RabbitMQ:Host"] ?? "localhost";
            var factory = new ConnectionFactory { HostName = host };

            try
            {
                var connection = await factory.CreateConnectionAsync(stoppingToken);
                var channel = await connection.CreateChannelAsync(
                    new CreateChannelOptions(false, false, null, null),
                    stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: "stock.updated",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>(),
                    noWait: false,
                    cancellationToken: stoppingToken);

                var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var data = JsonSerializer.Deserialize<StockUpdatedEvent>(json);
                        if (data != null)
                            await UpsertStockCacheAsync(data, stoppingToken);

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[RabbitMQ] Lỗi xử lý stock.updated");
                        await channel.BasicNackAsync(
                            ea.DeliveryTag,
                            multiple: false,
                            requeue: true,
                            cancellationToken: stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: "stock.updated",
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("[RabbitMQ] Đang lắng nghe stock.updated → ProductStockCaches");

                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
                channel.Dispose();
                connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMQ] Không kết nối được");
            }
        }

        private async Task UpsertStockCacheAsync(StockUpdatedEvent data, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            var cache = await db.ProductStockCaches
                .FirstOrDefaultAsync(p => p.ProductId == data.ProductId, cancellationToken);

            var stockStatus = Enum.TryParse<StockStatus>(data.StockStatus, true, out var parsed)
                ? parsed
                : (data.QuantityAvailable > 0 ? StockStatus.InStock : StockStatus.OutOfStock);

            if (cache == null)
            {
                cache = new ProductStockCache { ProductId = data.ProductId };
                db.ProductStockCaches.Add(cache);
            }

            cache.ProductCode = data.ProductCode;
            cache.ProductName = data.ProductName;
            cache.SellingPrice = data.SellingPrice;
            cache.QuantityAvailable = data.QuantityAvailable;
            cache.StockStatus = stockStatus;
            cache.LastUpdatedAt = data.UpdatedAt == default ? DateTime.UtcNow : data.UpdatedAt;

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[RabbitMQ] stock.updated: ProductId={P} Qty={Q}",
                data.ProductId, data.QuantityAvailable);
        }
    }
}
