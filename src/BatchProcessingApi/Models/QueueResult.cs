using System;

namespace BatchProcessingApi.Models;

public class QueueResult
{
    public string? MessageId { get; set; }
    public string? Name { get; set; }
    public DateTime? QueueDateTime { get; set; }
    public string? ErrorMessage { get; set; }
}
