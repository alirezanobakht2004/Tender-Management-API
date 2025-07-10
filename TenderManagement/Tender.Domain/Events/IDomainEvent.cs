// IDomainEvent.cs  (add to Tender.Domain, e.g., folder Common or Events)

using System;

namespace Tender.Domain.Events
{
    /// <summary>
    /// Marker interface for domain events; concrete events will
    /// carry their own data and timestamp.
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}
