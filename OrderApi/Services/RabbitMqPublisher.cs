using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace OrderApi.Services
{
    public class RabbitMqPublisher
    {
        private readonly IConfiguration _config;

        public RabbitMqPublisher(IConfiguration config)
        {
            _config = config;
        }

        public void Publish(string queueName, object message)
        {
            try
            {
                var host = _config["RabbitMQ:Host"] ?? "localhost";

                var factory = new RabbitMQ.Client.ConnectionFactory { HostName = host };
                // RabbitMQ.Client 7.x ưu tiên async; dùng async -> sync để giữ API hiện tại.
                using var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                using var channel = connection
                    .CreateChannelAsync(new RabbitMQ.Client.CreateChannelOptions(false, false, null, null), CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                // Declare durable queue (demo).
                channel.QueueDeclareAsync(
                    queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>(),
                    noWait: false,
                    cancellationToken: CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: queueName,
                    body: body,
                    cancellationToken: CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] Lỗi publish '{queueName}': {ex.Message}");
            }
        }
    }
}