using System;

namespace BatchProcessing.Common.Models;

public class MessageStatusUpdate
{
    public string? BatchId { get; }
    public MessageStatus Status { get; }
    public string? MessageId { get; }
    public string? ItemId { get; }

    private MessageStatusUpdate(string? batchId, MessageStatus status, string messageId, string itemId)
    {
        BatchId = batchId;
        Status = status;
        MessageId = messageId;
        ItemId = itemId;
    }

    public MessageStatusUpdate() { }

    public static MessageStatusUpdate Create(MessageStatus status, string messageId, string itemId, string? batchId = null)
    {
        return new MessageStatusUpdate(batchId, status, messageId, itemId);
    }
}

public struct MessageStatus
{
    public int Id { get; }
    public string Name { get; }

    private MessageStatus(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static MessageStatus Queued = new MessageStatus(0, "QUEUE");

    public static MessageStatus Processing = new MessageStatus(10, "PROCESSING");

    public static MessageStatus Completed = new MessageStatus(20, "COMPLETED");

    public static MessageStatus Failed = new MessageStatus(30, "FAILED");
}