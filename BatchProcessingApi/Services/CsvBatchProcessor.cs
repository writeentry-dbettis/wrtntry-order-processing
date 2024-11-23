using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using BatchProcessing.Common.Interfaces;
using BatchProcessing.Common.Models;
using BatchProcessingApi.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;

namespace BatchProcessingApi.Services;

public class CsvBatchProcessor : IBatchProcessor
{
    private readonly ILogger<CsvBatchProcessor> _logger;

    public CsvBatchProcessor(ILogger<CsvBatchProcessor> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<Tout> ProcessBatchFile<Tout>(Stream batchFile, [EnumeratorCancellation] CancellationToken cancellationToken) where Tout : class, INameableEntity
    {
        // read a list of rows from CSV into an enumerable of models
        using (var reader = new StreamReader(batchFile, Encoding.UTF8)) 
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) 
            {
                if (_mappings.TryGetValue(typeof(Tout), out var csvMap))
                {
                    csv.Context.RegisterClassMap(csvMap);
                }

                await foreach (var row in csv.GetRecordsAsync<Tout>(cancellationToken))
                {
                    yield return row;
                }
            }
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
