using System;

namespace BatchProcessingApi.Interfaces;

public interface IChatClient
{
    Task StatusChanged(string id, string status);
}
