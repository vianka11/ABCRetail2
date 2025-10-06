using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using Azure.Data.Tables;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class TableFunction
{
    private readonly TableStorageService _tableService;

    public TableFunction(TableStorageService tableService)
    {
        _tableService = tableService;
    }

    [Function("TableWrite")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "table/{tableName}")] HttpRequestData req,
        string tableName,
        FunctionContext context)
    {
        var log = context.GetLogger("TableWrite");
        log.LogInformation($"TableWrite triggered for {tableName}");

        var body = await new StreamReader(req.Body).ReadToEndAsync();
        if (string.IsNullOrEmpty(body))
            return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var pk = root.GetProperty("PartitionKey").GetString();
        var rk = root.GetProperty("RowKey").GetString();

        var tableClient = _tableService.GetTableClient(tableName);
        var entity = new TableEntity(pk, rk);

        foreach (var prop in root.EnumerateObject())
        {
            if (prop.Name == "PartitionKey" || prop.Name == "RowKey") continue;
            switch (prop.Value.ValueKind)
            {
                case JsonValueKind.Number:
                    if (prop.Value.TryGetInt64(out var i)) entity[prop.Name] = i;
                    else if (prop.Value.TryGetDouble(out var d)) entity[prop.Name] = d;
                    else entity[prop.Name] = prop.Value.GetRawText();
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    entity[prop.Name] = prop.Value.GetBoolean();
                    break;
                case JsonValueKind.String:
                    entity[prop.Name] = prop.Value.GetString();
                    break;
                default:
                    entity[prop.Name] = prop.Value.GetRawText();
                    break;
            }
        }

        await tableClient.UpsertEntityAsync(entity);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { message = "Entity upserted", table = tableName, partition = pk, row = rk });
        return response;
    }
}
