using ABCRetailFunctions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() 
    .ConfigureServices(services =>
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Blob Service
        var blobConn = config["StorageConnectionString"];
        var blobContainer = config["BlobContainer"] ?? "product-images";
        services.AddSingleton(new BlobService(blobConn, blobContainer));

        // Table Service
        var tableConn = config["TableConnectionString"];
        services.AddSingleton(new TableStorageService(tableConn));

        // Queue Service
        var queueConn = config["QueueConnectionString"];
        var queueName = config["OrdersQueueName"] ?? "orders";
        services.AddSingleton(new QueueService(queueConn, queueName));

        // File Share Service
        var fileConn = config["FileConnectionString"];
        var shareName = config["FileShareName"] ?? "fileshare";
        var directory = config["FileShareDirectory"] ?? "uploads";
        services.AddSingleton(new AzureFileShareService(fileConn, shareName, directory));
    })
    .Build();

host.Run();

// Mrzyglod, K., 2022. Azure for Developers. Birmingham: Packt.