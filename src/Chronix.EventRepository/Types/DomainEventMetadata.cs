using Chronix.EventRepository.Interfaces;

namespace Chronix.EventRepository.Types;

public record DomainEventMetadata(Guid AggregateId, DateTime DateOfEventUTC, Dictionary<string, object>? ObjectBag) : IDomainEventMetadata;
