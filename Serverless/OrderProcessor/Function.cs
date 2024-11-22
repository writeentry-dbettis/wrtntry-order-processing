using BatchProcessing.Common.Interfaces;
using BatchProcessing.Common.Models;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OrderProcessor;

[FunctionsStartup(typeof(Startup))]
public partial class Function : ICloudEventFunction<MessagePublishedData>
{
    private readonly IStorageProvider _storageProvider;
    private readonly OrderProcessorOptions _options;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions _jsonOptions = 
        new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    public Function(IStorageProvider storageProvider, IOptionsMonitor<OrderProcessorOptions> options, ILogger<Function> logger)
    {
        _storageProvider = storageProvider;
        _options = options.CurrentValue;
        _logger = logger;
    }      

    public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData data, CancellationToken cancellationToken)
    {
        // var apiClient = new HttpClient();
        // apiClient.BaseAddress = new Uri(_options.BatchProcessingApiUrl!);
        _logger.LogInformation($"Batch API URL: {_options.BatchProcessingApiUrl}");

        try 
        {
            // call API to set PROCESSING status            
            //_ = await apiClient.PostAsync("api/message-status", null, cancellationToken);
            var textData = data.Message?.TextData ?? "missing";

            Order? order = null;

            // parse message to order
            using (var jsonDocument = JsonDocument.Parse(textData))
            {
                var obj = jsonDocument.RootElement.GetProperty("data");
                order = obj.Deserialize<Order>(_jsonOptions);
            }

            _logger.LogInformation($"Processing order {order?.Id}...");

            // search for existing file
            //  - if found, add new revision
            //  - if not found, create new entry
            //var orderFile = await _storageProvider.GetFileAsync(order?.Id);

            await Task.Delay(100);

            // save file in cloud storage bucket
            //_storageProvider.UploadAsync(orderFile);

            // call API to set COMPLETED status 
            //_ = await apiClient.PostAsync("api/message-status", null, cancellationToken);
            _logger.LogInformation($"Completed order {order?.Id}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the order");
            //_ = await apiClient.PostAsync("api/message-status", null, cancellationToken);
        }
    }
}
