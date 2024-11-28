using System.Numerics;

namespace BatchProcessingApi.Models;

public class QueueTopic
{
    public string TopicId { get; }
    public string Name { get; }

    private QueueTopic(string topicId, string name)
    {
        TopicId = topicId;
        Name = name;
    }

    public static QueueTopic OrderProcessing = new QueueTopic("order-processing", "Order Processing Queue");

    public static implicit operator string(QueueTopic q) => q.TopicId;
}