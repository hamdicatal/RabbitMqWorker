using System.Net;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsumerApp
{
    public class ResponseLog
    {
        public string ServiceName { get; set; } = "RabbitListener";
        public string Url { get; set; }
        public int StatusCode { get; set; }
    }

    static class Consumer
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "urls",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var url = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received {url}");

                var response = await HeadRequest(url);

                LogResponse(url, response);

            };
            channel.BasicConsume(queue: "urls",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void LogResponse(string url, HttpResponseMessage response)
        {
            ResponseLog responseLog = new ResponseLog();
            responseLog.StatusCode = response.StatusCode.GetHashCode();
            responseLog.Url = url;

            string log = JsonSerializer.Serialize(responseLog);
        }

        private static async Task<HttpResponseMessage> HeadRequest(string url)
        {
            return await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
        }
    }
}