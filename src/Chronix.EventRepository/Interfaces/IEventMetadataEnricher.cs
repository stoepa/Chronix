namespace Chronix.EventRepository.Interfaces;

public interface IEventMetadataEnricher
{
    public IDomainEventMetadata Enrich(IDomainEventMetadata domainEventMetadata);
}
