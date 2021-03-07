using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMvcTest
{
    public class RabbitSender
    {
        private static IConnection CreateConnection()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            return factory.CreateConnection();
        }
        public static void Send(string message)
        {
            using (var connection = CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queueName = channel.QueueDeclare().QueueName;
                var routingKey = "tour.booked";
                channel.ConfirmSelect();

                var pendingConfirms = new ConcurrentDictionary<ulong, string>();

                void cleanPendingConfirms(ulong sequenceNumber, bool multiple)
                {
                    if (multiple)
                    {
                        var confirmed = pendingConfirms.Where(k => k.Key <= sequenceNumber);
                        foreach (var entry in confirmed)
                        {
                            pendingConfirms.TryRemove(entry.Key, out _);
                        }
                    }
                    else
                    {
                        pendingConfirms.TryRemove(sequenceNumber, out _);
                    }
                }

                //Provide callbacks for message confirms
                channel.BasicAcks += (sender, ea) => cleanPendingConfirms(ea.DeliveryTag, ea.Multiple);
                channel.BasicNacks += (sender, ea) =>
                {
                    pendingConfirms.TryGetValue(ea.DeliveryTag, out string body);
                    //ToDo if message fails
                    cleanPendingConfirms(ea.DeliveryTag, ea.Multiple);
                };

                //Sending message
                var timer = new Stopwatch();
                timer.Start();
                pendingConfirms.TryAdd(channel.NextPublishSeqNo, message);
                channel.BasicPublish(
                    exchange: "tour_booking",
                    routingKey: routingKey,
                    basicProperties: null,
                    body: Encoding.UTF8.GetBytes(message)
                    );

                if (!WaitUntil(60, () => pendingConfirms.IsEmpty))
                    throw new Exception("All messages could not be confirmed in 60 seconds");

                timer.Stop();
                Console.WriteLine($"Published <{message}> and handled confirm asynchronously {timer.ElapsedMilliseconds:N0} ms");
                //ToDo log?
            }
        }
        private static bool WaitUntil(int numberOfSeconds, Func<bool> condition)
        {
            int waited = 0;
            while (!condition() && waited < numberOfSeconds * 1000)
            {
                Thread.Sleep(100);
                waited += 100;
            }

            return condition();
        }
    }
}