using System;
using BatchProcessing.Common.Interfaces;
using BatchProcessingApi.Models;

namespace BatchProcessingApi.Interfaces;

public interface IPublishService
{
    IAsyncEnumerable<QueueResult> PublishMessageToTopic<T>(string topicId, string batchId, CancellationToken cancellationToken, params T[] objs) where T : INameableEntity;
}
