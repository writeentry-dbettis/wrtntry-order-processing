using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using BatchProcessingApi.Interfaces;
using BatchProcessingApi.Models;
using CsvHelper;

namespace BatchProcessingApi.Services;

public class CsvBatchProcessor : IBatchProcessor
{
    private readonly IPublishService _publishService;
    private readonly ILogger<CsvBatchProcessor> _logger;

    public CsvBatchProcessor(IPublishService publishService, ILogger<CsvBatchProcessor> logger)
    {
        _publishService = publishService;
        _logger = logger;
    }

    public async IAsyncEnumerable<QueueResult> ProcessBatchFile<Tout>(Stream batchFile, string topicId, 
        [EnumeratorCancellation] CancellationToken cancellationToken) where Tout : INameableEntity
    {
        var rows = new List<Tout>();

        // read a list of rows from CSV into an enumerable of models
        using (var reader = new StreamReader(batchFile, Encoding.UTF8)) 
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) 
            {
                rows = csv.GetRecords<Tout>().ToList();
            }
        }

        // publish each row as a message to the queue
        var published = 0;
        foreach (var row in rows)
        {
            string? messageId = string.Empty
                , errorMessage = null;

            try
            {
                messageId = await _publishService.PublishMessageToTopic(row, topicId, cancellationToken);
                published++;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError(ex, "Error publishing message");
            }

            yield return new QueueResult
            {
                MessageId = messageId,
                Name = row.Name(),
                QueueDateTime = DateTime.UtcNow,
                ErrorMessage = errorMessage
            };
        }
    }
}
