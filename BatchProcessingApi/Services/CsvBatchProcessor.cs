using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using BatchProcessing.Common;
using BatchProcessing.Common.Interfaces;
using BatchProcessing.Common.Models;
using BatchProcessingApi.Interfaces;
using BatchProcessingApi.Models;
using CsvHelper;
using CsvHelper.Configuration;

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
        string batchId, [EnumeratorCancellation] CancellationToken cancellationToken) where Tout : class, INameableEntity
    {
        var rows = new List<Tout>();

        // read a list of rows from CSV into an enumerable of models
        using (var reader = new StreamReader(batchFile, Encoding.UTF8)) 
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) 
            {
                if (_mappings.TryGetValue(typeof(Tout), out var csvMap))
                {
                    csv.Context.RegisterClassMap(csvMap);
                }

                rows = csv.GetRecords<Tout>().ToList();
            }
        }

        // publish each row as a message to the queue
        await foreach (var message in _publishService.PublishMessageToTopic<Tout>(topicId, batchId, cancellationToken, [.. rows]))
        {
            yield return message;
        }
    }

    private static Dictionary<Type, Type> _mappings = 
        new Dictionary<Type, Type>()
        {
            { typeof(Order), typeof(OrderMap)}
        };

    class OrderMap : ClassMap<Order>
    {
        public OrderMap()
        {
            Map(o => o.Id).Name("Id");
            Map(o => o.PONumber).Name("PO");
            Map(o => o.TotalAmount).Name("Total");
            Map(o => o.Tax).Name("Tax");
            Map(o => o.CreatedDate).Name("Date");
        }
    }
}
