using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderApi.Services
{
    public class StockConsumerService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<StockConsumerService> _logger;

        // Lưu tồn kho nhận từ N1: productId → stock
        public static Dictionary<int, int> StockCache { get; } = new();

        public StockConsumerService(
            IConfiguration config,
            ILogger<StockConsumerService> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var host = _config["RabbitMQ:Host"] ?? "localhost";
            var factory = new ConnectionFactory { HostName = host };

            try
            {
                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();

                channel.QueueDeclare(
                    queue: "stock.updated",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var data = JsonSerializer.Deserialize<StockUpdatedEvent>(json);
                        if (data != null)
                        {
                            StockCache[data.ProductId] = data.Stock;
                            _logger.LogInformation(
                                "[RabbitMQ] stock.updated: ProductId={P} Stock={S}",
                                data.ProductId, data.Stock);
                        }
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[RabbitMQ] Lỗi xử lý: {M}", ex.Message);
                        channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                channel.BasicConsume(
                    queue: "stock.updated",
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation("[RabbitMQ] Đang lắng nghe stock.updated...");

                stoppingToken.WaitHandle.WaitOne();
                channel.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError("[RabbitMQ] Không kết nối được: {M}", ex.Message);
            }

            return Task.CompletedTask;
        }
    }

    public class StockUpdatedEvent
    {
        public int ProductId { get; set; }
        public int Stock { get; set; }
    }
}