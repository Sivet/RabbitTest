using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMvcTest
{
    public class RabbitSender
    {

        public static void Send(string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: "tour_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );
                
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                        routingKey: "tour_queue",
                        basicProperties: properties,
                        body: body
                        );
            }
        }
    }
}