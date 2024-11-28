using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BatchProcessing.Common.Interfaces;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderProcessor.Services;

public class GcpCloudStorageProvider : IStorageProvider
{
    private readonly GcpCloudStorageProviderOptions _options;
    private readonly ILogger<GcpCloudStorageProvider> _logger;

    public GcpCloudStorageProvider(IOptionsMonitor<GcpCloudStorageProviderOptions> options, ILogger<GcpCloudStorageProvider> logger)
    {
        _options = options.CurrentValue;
        _logger = logger;
    }

    public async Task UploadStringAsTextFileAsync(string fileName, string content, string contentType, Encoding encoding, CancellationToken cancellationToken)
    {
        using (var storageClient = StorageClient.Create())
        {
            var targetBucket = storageClient.GetBucketAsync(_options.OrderBucketName, cancellationToken: cancellationToken);

            if (targetBucket == null)
            {
                targetBucket = storageClient.CreateBucketAsync(
                    _options.ProjectId, 
                    _options.OrderBucketName, 
                    cancellationToken: cancellationToken);
            }

            var binaryContent = encoding.GetBytes(content);

            using (var textStream = new MemoryStream(binaryContent))
            {
                var obj = await storageClient.UploadObjectAsync(
                    _options.OrderBucketName,
                    fileName,
                    contentType,
                    textStream,
                    cancellationToken: cancellationToken
                );
            }            
        }
    }
}

public class GcpCloudStorageProviderOptions
{
    public string? ProjectId { get; set; }
    public string OrderBucketName { get; set; } = "orders-np";
}
