using System;

namespace BatchProcessing.Common.Models.Requests;

public class MessageStatusUpdateRequest
{
    public string? BatchId { get; set;}
    public MessageStatusRequest? Status { get; set; }
    public string? MessageId { get; set; }
    public string? ItemId { get; set; }
}

public class MessageStatusRequest
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
