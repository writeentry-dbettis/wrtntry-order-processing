using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BatchProcessing.Common.Interfaces;
using BatchProcessing.Common.Models;
using BatchProcessingApi.Interfaces;
using BatchProcessingApi.Models;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

namespace BatchProcessingApi.Services;

public class QueuePublishService : IPublishService
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions() 
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly QueuePublishServiceConfiguration _options;
    private readonly ILogger<QueuePublishService> _logger;

    public QueuePublishService(IOptionsMonitor<QueuePublishServiceConfiguration> options, ILogger<QueuePublishService> logger)
    {
        _options = options.CurrentValue;
        _logger = logger;
    }

    public async IAsyncEnumerable<QueueResult> PublishMessageToTopic<T>(string topicId, string batchId, [EnumeratorCancellation] CancellationToken cancellationToken, params T[] objs) where T : class, INameableEntity
    {
        var topicName = new TopicName(_options.ProjectId, topicId);

        var retVal = new List<string>();

        await using (var publisher = await PublisherClient.CreateAsync(topicName))
        {
            foreach (var obj in objs)
            {
                var messageId = "";
                string? errorMessage = null;

                try 
                {
                    var message = QueueMessage<T>.CreateForEnvironment(obj, _options.EnvironmentName);

                    var messageJson = JsonSerializer.Serialize(message, _jsonSerializerOptions);

                    var pubMessage = new PubsubMessage
                    {
                        Data = Google.Protobuf.ByteString.CopyFromUtf8(messageJson)
                    };

                    pubMessage.Attributes.Add("environment", _options.EnvironmentName);

                    if (!string.IsNullOrWhiteSpace(batchId))
                    {
                        pubMessage.Attributes.Add("batchId", batchId);
                    }                    

                    messageId = await publisher.PublishAsync(pubMessage);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    _logger.LogError(ex, $"An error occurred while queueing item {obj.Name}.");
                }

                yield return new QueueResult
                {
                    MessageId = messageId,
                    Name = obj.Name(),
                    QueueDateTime = DateTime.UtcNow,
                    ErrorMessage = errorMessage
                };
            }            
        }       
    }
}


public class QueuePublishServiceConfiguration
{
    public string? ProjectId { get; set; }
    public string EnvironmentName { get; set; } = "development";
}
