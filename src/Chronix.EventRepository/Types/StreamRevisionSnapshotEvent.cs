namespace Chronix.EventRepository.Types;

public record StreamRevisionSnapshotEvent(string PreviousStreamName, string OriginalStreamName, DateTime DateOfSnapshot, long Version, string Snapshot) : DomainEvent(nameof(StreamRevisionSnapshotEvent));
