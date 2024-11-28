using System;

namespace BatchProcessing.Common.Interfaces;

public interface INameableEntity
{
    string Id { get;}
    string Name();
}
