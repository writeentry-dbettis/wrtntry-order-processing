using System.Collections.Concurrent;
using System.Text.Json;
using BatchProcessing.Common.Interfaces;
using BatchProcessing.Common.Models;
using BatchProcessingApi.Hubs;
using BatchProcessingApi.Interfaces;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace BatchProcessingApi.Services;

public class QueuePublishService : BackgroundService, IPublishService
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly QueuePublishServiceConfiguration _options;
    private readonly ILogger<QueuePublishService> _logger;

    private static readonly ConcurrentQueue<QueueRequest<INameableEntity>> _messageQueue =
        new ConcurrentQueue<QueueRequest<INameableEntity>>();

    private static Dictionary<int, PublisherClient> _publisherClient =
        new Dictionary<int, PublisherClient>();

    public QueuePublishService(IHubContext<ChatHub, IChatClient> hubContext, IOptionsMonitor<QueuePublishServiceConfiguration> options, ILogger<QueuePublishService> logger)
    {
        _hubContext = hubContext;
        _options = options.CurrentValue;
        _logger = logger;
    }

    public async Task<int> PublishMessageToTopic<Tin>(string topicId, string batchId, CancellationToken cancellationToken, params Tin[] objs) where Tin : class, INameableEntity
    {
        var retVal = 0;

        for (int i = 0; i < objs.Length; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            
            var request = QueueRequest<INameableEntity>.Create(topicId, batchId, objs[i] as INameableEntity);

            _messageQueue.Enqueue(request);

            retVal++;
        }

        return await Task.FromResult(retVal);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            if (_messageQueue.TryDequeue(out var queueMessage))
            {
                var failed = false;

                var topicName = new TopicName(_options.ProjectId, queueMessage.TopicId);

                if (!_publisherClient.TryGetValue(topicName.GetHashCode(), out var publisher))
                {
                    publisher = await PublisherClient.CreateAsync(topicName);
                    _publisherClient.Add(topicName.GetHashCode(), publisher);
                }

                try
                {
                    var message = QueueMessage<INameableEntity>
                        .CreateForEnvironment(queueMessage.Data, _options.EnvironmentName);

                    var messageJson = JsonSerializer.Serialize(message, _jsonSerializerOptions);

                    var pubMessage = new PubsubMessage
                    {
                        Data = Google.Protobuf.ByteString.CopyFromUtf8(messageJson)
                    };

                    pubMessage.Attributes.Add("environment", _options.EnvironmentName);

                    if (!string.IsNullOrWhiteSpace(queueMessage.BatchId))
                    {
                        pubMessage.Attributes.Add("batchId", queueMessage.BatchId);
                    }

                    var messageId = await publisher!.PublishAsync(pubMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while queueing item {queueMessage.Data.Name()}.");
                    failed = true;
                }

                if (!string.IsNullOrEmpty(queueMessage.BatchId))
                {
                    await _hubContext.Clients.Groups(queueMessage.BatchId).StatusChanged(
                        queueMessage.Data.Id,
                        (failed ? MessageStatus.Failed : MessageStatus.Queued).Name
                    );
                }
            }
            else
            {
                // clear out clients in between queueing batches
                foreach (var publisher in _publisherClient)
                {
                    await publisher.Value.DisposeAsync();
                }
                _publisherClient.Clear();

                await Task.Delay(1000);
            }
        }
    }
}

public class QueueRequest<T> where T : class, INameableEntity
{
    public string TopicId { get; }
    public string? BatchId { get; }
    public T Data { get; }

    private QueueRequest(string topicId, string batchId, T data)
    {
        TopicId = topicId;
        BatchId = batchId;
        Data = data;
    }

    public static QueueRequest<Tin> Create<Tin>(string topicId, string batchId, Tin data) where Tin : class, INameableEntity
    {
        return new QueueRequest<Tin>(topicId, batchId, data);
    }
}


public class QueuePublishServiceConfiguration
{
    public string? ProjectId { get; set; }
    public string EnvironmentName { get; set; } = "development";
}
