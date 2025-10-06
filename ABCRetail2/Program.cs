using ABCRetail2.Services;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AzureStorageOptions>(
    builder.Configuration.GetSection("AzureStorage"));

builder.Services.AddSingleton<TableStorageService>();      // Service for Table Storage
builder.Services.AddSingleton<BlobService>();              // Service for Blob Storage
builder.Services.AddSingleton<QueueService>();             // Service for Queue Storage
builder.Services.AddSingleton<AzureFileShareService>();    // Service for File Share Storage

builder.Services.AddControllersWithViews();

var cultureInfo = new CultureInfo("en-ZA");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


public sealed class AzureStorageOptions
{
    public string ConnectionString { get; set; } = "DefaultEndpointsProtocol=https;AccountName=st10395268storage;AccountKey=d7XuLhOLpreSXylHV5X7SGSBk8FiTsmKKfaVesHMkznNg6JCdSDwZB8sAgpN3cmkf8K5fB7LMABi+AStgBXVcw==;EndpointSuffix=core.windows.net";

    // Table Storage
    public TablesOptions Tables { get; set; } = new();

    // Blob Storage
    public BlobOptions Blob { get; set; } = new();

    // Queue Storage
    public QueueOptions Queues { get; set; } = new();

    // File Share
    public FileShareOptions FileShare { get; set; } = new();

    public sealed class TablesOptions
    {
        public string Customer { get; set; } = "Customers";
        public string Product { get; set; } = "Products";
        public string Order { get; set; } = "Orders";
    }

    public sealed class BlobOptions
    {
        public string Container { get; set; } = "product-images";
    }

    public sealed class QueueOptions
    {
        public string Orders { get; set; } = "orders-queue";
    }

    public sealed class FileShareOptions
    {
        public string ShareName { get; set; } = "contracts";   // Name of file share
        public string Directory { get; set; } = "uploads";     // Directory within the share for uploads
    }
}
// Mrzyglod, K., 2022. Azure for Developers. Birmingham: Packt.