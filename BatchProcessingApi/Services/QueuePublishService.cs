using System;
using System.Text.Json;
using BatchProcessingApi.Interfaces;
using Google.Cloud.PubSub.V1;
using Google.Protobuf.Collections;
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

    public async Task<string> PublishMessageToTopic<T>(T obj, string topicId, CancellationToken cancellationToken)
    {
        var message = new 
        {
            Type = typeof(T),
            Environment = _options.EnvironmentName,
            Data = obj,
        };

        var messageJson = JsonSerializer.Serialize(message, _jsonSerializerOptions);

        var topicName = new TopicName(_options.ProjectId, topicId);

        string messageId = "---";

        await using (var publisher = await PublisherClient.CreateAsync(topicName))
        {
             var pubMessage = new PubsubMessage
            {
                Data = Google.Protobuf.ByteString.CopyFromUtf8(messageJson)
            };
            pubMessage.Attributes.Add("environment", _options.EnvironmentName);

            messageId = await publisher.PublishAsync(pubMessage);
        }       

        _logger.LogInformation($"Published {messageId} to topic {topicName}");

        return messageId;
    }
}

public class QueuePublishServiceConfiguration
{
    public string? QueueName { get; set; }
    public Uri? BaseUrl { get; set; }
    public string? ProjectId { get; set; }
    public string EnvironmentName { get; set; } = "development";
}
