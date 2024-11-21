using System;
using BatchProcessing.Common.Interfaces;
using BatchProcessingApi.Models;

namespace BatchProcessingApi.Interfaces;

public interface IBatchProcessor
{
    IAsyncEnumerable<QueueResult> ProcessBatchFile<Tout>(Stream batchFile, string topicId, string batchId, CancellationToken cancellationToken) where Tout : INameableEntity;
}
