namespace Chronix.EventRepository.Interfaces;

public interface IDomainEventMetadata
{
    public Guid AggregateId { get; init; }
    public DateTime DateOfEventUTC { get; init; }
    public Dictionary<string, object>? ObjectBag { get; init; }
}
