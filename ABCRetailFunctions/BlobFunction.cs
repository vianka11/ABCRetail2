using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class BlobFunction
{
    private readonly BlobService _blobService;

    public BlobFunction(BlobService blobService)
    {
        _blobService = blobService;
    }

    [Function("BlobUpload")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "blob/upload")] HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger("BlobUpload");
        log.LogInformation("BlobUpload triggered");

        var body = await new StreamReader(req.Body).ReadToEndAsync();
        if (string.IsNullOrEmpty(body))
            return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var fileName = root.GetProperty("FileName").GetString();
        var base64 = root.GetProperty("ContentBase64").GetString();

        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(base64))
            return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);

        var bytes = Convert.FromBase64String(base64);
        var blobUrl = await _blobService.UploadAsync(fileName, bytes);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { blobUrl });
        return response;
    }
}
