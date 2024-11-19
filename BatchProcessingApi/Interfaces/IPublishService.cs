using System;

namespace BatchProcessingApi.Interfaces;

public interface IPublishService
{
    Task<string> PublishMessageToTopic<T>(T obj, string topicId, CancellationToken cancellationToken);
}
