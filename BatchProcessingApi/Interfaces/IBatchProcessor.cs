using BatchProcessing.Common.Interfaces;

namespace BatchProcessingApi.Interfaces;

public interface IBatchProcessor
{
    IAsyncEnumerable<Tout> ProcessBatchFile<Tout>(Stream batchFile, CancellationToken cancellationToken) where Tout : class, INameableEntity;
}
