using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Abstractions
{
    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredOnUtc { get; }
    }
}