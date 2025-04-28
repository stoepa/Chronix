namespace Chronix.EventRepository.Types;

public record StreamRevisionIndexEvent(Guid AggregateId, string LatestStreamName, DateTime DateOfRevision, long Version) : DomainEvent(nameof(StreamRevisionIndexEvent));
