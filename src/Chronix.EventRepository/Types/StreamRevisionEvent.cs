namespace Chronix.EventRepository.Types;

public record StreamRevisionEvent(string NextStreamName, string OriginalStreamName, DateTime DateOfRevision, long Version) : DomainEvent(nameof(StreamRevisionEvent));
