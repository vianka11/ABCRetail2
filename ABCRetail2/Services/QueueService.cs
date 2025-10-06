using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;

namespace ABCRetail2.Services
{
    public class QueueService
    {
        private readonly QueueClient _ordersQueue;

        /// <summary>
        /// Constructor - initializes the queue client and ensures the queue exists
        /// </summary>
        public QueueService(IOptions<AzureStorageOptions> options)
        {
            var o = options.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(o.Queues?.Orders))
                throw new ArgumentException("Orders queue name must be configured in AzureStorageOptions.");

            _ordersQueue = new QueueClient(o.ConnectionString, o.Queues.Orders);

            try
            {
                _ordersQueue.CreateIfNotExists();
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"[QueueService] Failed to create queue: {ex.Message}");
            }
        }

        /// <summary>
        /// Enqueues a message to the Orders queue
        /// </summary>
        /// <param name="payload">Any serializable object representing the order</param>
        public async Task EnqueueOrderEventAsync(object payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            try
            {
                var json = JsonSerializer.Serialize(payload);
                var base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

                await _ordersQueue.SendMessageAsync(base64Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QueueService] Failed to enqueue message: {ex.Message}");
                throw;
            }
        }
    }
}
