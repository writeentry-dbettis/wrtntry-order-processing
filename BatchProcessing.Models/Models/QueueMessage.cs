using System;

namespace BatchProcessing.Common.Models;

public class QueueMessage<T> where T : class
{
    public string Type { get; }
    public string Environment { get; }
    public T? Data { get; }

    private QueueMessage(string type, string environment, T data)
    {
        Type = type;
        Environment = environment;
        Data = data;
    }

    public static QueueMessage<Tin> CreateForEnvironment<Tin>(Tin data, string environment) where Tin : class
    {
        var typeName = typeof(Tin).FullName!;

        var retVal = new QueueMessage<Tin>(typeName, environment, data);

        return retVal;
    }
}
