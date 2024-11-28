using System;
using System.Text;

namespace BatchProcessing.Common.Interfaces;

public interface IStorageProvider
{
    Task UploadStringAsTextFileAsync(string fileName, string content, string contentType, Encoding encoding, CancellationToken cancellationToken);
}
