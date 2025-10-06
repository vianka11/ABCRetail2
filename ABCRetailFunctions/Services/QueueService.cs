using Azure.Storage.Queues;
using System.Text.Json;

namespace ABCRetailFunctions.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task EnqueueAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var base64Message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            await _queueClient.SendMessageAsync(base64Message);
        }
    }
}
