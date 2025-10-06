using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;

namespace QueueFunctionBasics;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly string _storageConnectionString;
    private TableClient _tableClient;
    private BlobContainerClient _blobContainerClient;
    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
        _storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=10433939storage;AccountKey=T+cF1llgLLpDVoHRvYB2euojWeH561LY3k5ALbmnupU3+eV9QDoAxRcrgRVivxywRRq+NVWjptsL+AStM3ttkw==;EndpointSuffix=core.windows.net";
        //Create table client
        var serviceClient = new TableServiceClient(_storageConnectionString);
        _tableClient = serviceClient.GetTableClient("orders");

        _blobContainerClient = new BlobContainerClient(_storageConnectionString, "products");
        _blobContainerClient.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
    }
    //
    [Function(nameof(QueueOrdersSender))]
    public async Task QueueOrdersSender([QueueTrigger("orders", Connection = "connection")] QueueMessage message)
    {
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", message.MessageText);

        //Craete table if doesn't exist
        await _tableClient.CreateIfNotExistsAsync();

        var order = JsonSerializer.Deserialize<OrderEntity>(message.MessageText);

        if (order == null)
        {
            _logger.LogError("Failed to deserialize JSON message.");
            return;
        }

        order.RowKey = Guid.NewGuid().ToString();
        order.PartitionKey = "Orders";

        _logger.LogInformation($"Saving entity with RowKey: {order.RowKey}");

        await _tableClient.AddEntityAsync(order);
        _logger.LogInformation("Successfully saved order to table.");
    }
    //
    [Function("GetOrders")]
    public async Task<HttpResponseData> GetOrders(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request to get all orders.");

        try
        {
            var orders = await _tableClient.QueryAsync<OrderEntity>().ToListAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(orders);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query table storage.");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An error occured while retrieving data from the tbale.");
            return response;
        }
    }
    //
    [Function("AddProductWithImage")]
    public async Task<HttpResponseData> AddProductWithImage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products-with-image")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function to add product with image recieved a request.");
        var newProduct = new OrderEntity();
        string? uploadedBlobUrl = null;
        //Parse the multipart form data
        var multipartReader = new MultipartReader(
            req.Headers.GetValues("Content-type").First().Split(';')[1].Trim().Split('=')[1], req.Body);
        var section = await multipartReader.ReadNextSectionAsync();

        while (section != null)
        {
            var contentDisposition = section.Headers["Content-Disposition"].ToString();
            var name = contentDisposition.Split(';')[1].Trim().Split('=')[1].Trim('"');

            if (name == "Name" || name == "Email")
            {
                var value = await new StreamReader(section.Body).ReadToEndAsync();
                if (name == "Name") newProduct.Name = value;
                if (name == "Email") newProduct.Email = value;
            }
            else if (name == "Image")
            {
                var fileName = contentDisposition.Split(';')[2].Trim().Split('=')[1].Trim('"');
                var uniqueFileName = $"{Guid.NewGuid()}-{Path.GetFileName(fileName)}";
                var blobClient = _blobContainerClient.GetBlobClient(uniqueFileName);

                //Upload
                await blobClient.UploadAsync(section.Body, true);
                uploadedBlobUrl = blobClient.Uri.ToString();
            }
            section = await multipartReader.ReadNextSectionAsync();
        }
        //Validate and save to table storage
        if (string.IsNullOrEmpty(newProduct.Name) || string.IsNullOrEmpty(newProduct.Email) || string.IsNullOrEmpty(uploadedBlobUrl))
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        newProduct.PartitionKey = "Orders";
        newProduct.RowKey = Guid.NewGuid().ToString();
        newProduct.PicUrl = uploadedBlobUrl;

        await _tableClient.AddEntityAsync(newProduct);
        _logger.LogInformation($"Successfully added {newProduct.Name} and uploaded picture.");

        return req.CreateResponse(HttpStatusCode.Created);
    }
}