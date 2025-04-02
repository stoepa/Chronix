namespace Chronix.EventRepository.Types;

public record DomainEventData(DomainEvent DomainEvent, DomainEventMetadata DomainEventMetadata);
