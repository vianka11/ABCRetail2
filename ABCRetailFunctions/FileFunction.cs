using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using System.Threading.Tasks;
using System.IO; 
using System.Net; 

public class FileShareFunction
{
    private readonly AzureFileShareService _fileShareService;

    public FileShareFunction(AzureFileShareService fileShareService)
    {
        _fileShareService = fileShareService;
    }

    [Function("FileShareUpload")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "files/upload")] HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger("FileShareUpload");

        // Read request body as a stream and parse the multipart form manually
        var formCollection = await MultipartRequestHelper.ReadMultipartFormAsync(req);

        var file = formCollection.Files.GetFile("file");
        if (file == null) return req.CreateResponse(HttpStatusCode.BadRequest);

        var fileUrl = await _fileShareService.UploadAsync(file);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { fileUrl });
        return response;
    }
}

// Helper class to parse multipart form data from HttpRequestData
public static class MultipartRequestHelper
{
    public static async Task<IFormCollection> ReadMultipartFormAsync(HttpRequestData req)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = req.Body;
        foreach (var header in req.Headers)
        {
            httpContext.Request.Headers[header.Key] = new Microsoft.Extensions.Primitives.StringValues(header.Value.ToArray());
        }
        return await httpContext.Request.ReadFormAsync();
    }
}