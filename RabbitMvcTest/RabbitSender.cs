using System.Text;
using RabbitMQ.Client;

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
                channel.ExchangeDeclare(exchange: "tour_booking",
                                        type: "topic");

                var routingKey = "tour.booked";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "tour_booking",
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);
                //Console.WriteLine(" [x] Sent {0}", message);
            }

            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();
        }
    }
}