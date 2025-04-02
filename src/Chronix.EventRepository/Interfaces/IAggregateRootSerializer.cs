using Chronix.EventRepository.Types;
using EventStore.Client;

namespace Chronix.EventRepository.Interfaces;

public interface IAggregateRootSerializer<T> where T : AggregateRoot
{
    public EventData Serialize(DomainEvent domainEvent, IDomainEventMetadata domainEventMetadata);
    public DomainEvent Deserializer(string eventName, ReadOnlySpan<byte> eventSpan);
}
