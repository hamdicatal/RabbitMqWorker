using System.Text;
using RabbitMQ.Client;

namespace PublisherApp
{
    static class Publisher
    {
        static void PublishUrl(IModel channel, string url)
        {
            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "urls",
                                 basicProperties: null,
                                 body: Encoding.UTF8.GetBytes(url));

            Console.WriteLine($" [x] Sent {url}");
        }

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "urls",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            PublishUrl(channel, "https://jsonplaceholder.typicode.com/users");
            // PublishUrl(channel, "https://twitter.com/");
            // PublishUrl(channel, "https://youtube.com/");

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
            
        }
    }
}
