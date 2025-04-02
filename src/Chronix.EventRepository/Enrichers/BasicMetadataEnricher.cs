using Chronix.EventRepository.Interfaces;

namespace Chronix.EventRepository.Enrichers;

public class BasicMetadataEnricher : IEventMetadataEnricher
{
    public IDomainEventMetadata Enrich(IDomainEventMetadata domainEventMetadata)
    {
        return domainEventMetadata;
    }
}
