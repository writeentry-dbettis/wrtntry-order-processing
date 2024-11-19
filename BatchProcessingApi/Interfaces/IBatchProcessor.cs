using System;
using BatchProcessingApi.Models;

namespace BatchProcessingApi.Interfaces;

public interface IBatchProcessor
{
    IAsyncEnumerable<QueueResult> ProcessBatchFile<Tout>(Stream batchFile, string topicId, CancellationToken cancellationToken) where Tout : INameableEntity;
}
