using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using System.Text.Json;
using System.Threading.Tasks;

public class QueueFunction
{
    private readonly QueueService _queueService;

    public QueueFunction(QueueService queueService)
    {
        _queueService = queueService;
    }

    [Function("OrdersQueueProcessor")]
    public async Task Run([QueueTrigger("%OrdersQueueName%", Connection = "QueueConnectionString")] string queueMessage,
                          FunctionContext context)
    {
        var log = context.GetLogger("OrdersQueueProcessor");
        log.LogInformation($"Queue triggered: {queueMessage}");

        using var doc = JsonDocument.Parse(queueMessage);
        var root = doc.RootElement;
        var type = root.GetProperty("MessageType").GetString();

        await _queueService.EnqueueAsync(root);
        log.LogInformation($"Message processed: {type}");
    }
}
