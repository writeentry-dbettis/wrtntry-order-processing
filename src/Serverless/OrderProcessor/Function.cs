using BatchProcessing.Common.Interfaces;
using BatchProcessing.Common.Models;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Json;
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
        var apiClient = new HttpClient
        {
            BaseAddress = new Uri(_options.BatchProcessingApiUrl!)
        };

        _logger.LogInformation($"Batch API URL: {_options.BatchProcessingApiUrl}");

        var failed = false;
        string? itemId = string.Empty,
            batchId = null;

        try 
        {
            var textData = data.Message?.TextData ?? "missing";
            batchId = data.Message?.Attributes["batchId"];

            _logger.LogInformation($"Processing order from batch {batchId}...");

            Order? order = null;

            // parse message to order
            using (var jsonDocument = JsonDocument.Parse(textData))
            {
                var obj = jsonDocument.RootElement.GetProperty("data");
                order = obj.Deserialize<Order>(_jsonOptions);
            }

            itemId = order!.Id;

            // call API to set PROCESSING status 
            var processingMessage = MessageStatusUpdate.Create(
                MessageStatus.Processing,
                data.Message?.MessageId!,
                itemId,
                batchId);           

            var processingContent = JsonContent.Create(processingMessage, options: _jsonOptions);

            _ = await apiClient.PostAsync("api/message-status", processingContent, cancellationToken);

            _logger.LogInformation($"Processing order {order?.Id}...");

            // search for existing file
            //  - if found, add new revision
            //  - if not found, create new entry
            //var orderFile = await _storageProvider.GetFileAsync(order?.Id);

            var random = new Random();
            var delay = random.Next(500, 3000);
            await Task.Delay(delay);

            // save file in cloud storage bucket
            //_storageProvider.UploadAsync(orderFile);

            _logger.LogInformation($"Completed order {order?.Id}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the order");
            failed = true;
        }

        // call API to set COMPLETED status 
        var messageStatus = MessageStatusUpdate.Create(
            failed ? MessageStatus.Failed : MessageStatus.Completed,
            data.Message?.MessageId!,
            itemId,
            batchId);

        var completedContent = JsonContent.Create(messageStatus, options: _jsonOptions);

        _ = await apiClient.PostAsync("api/message-status", completedContent, cancellationToken);
    }
}
