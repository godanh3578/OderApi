using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

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

                var factory = new ConnectionFactory { HostName = host };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);
                var props = channel.CreateBasicProperties();
                props.Persistent = true;

                channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: props,
                    body: body
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] Lỗi publish '{queueName}': {ex.Message}");
            }
        }
    }
}