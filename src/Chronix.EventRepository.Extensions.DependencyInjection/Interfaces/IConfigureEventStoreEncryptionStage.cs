using Chronix.EventRepository.Interfaces;
using Chronix.EventRepository.Options;
using Chronix.EventRepository.Types;

namespace Chronix.EventRepository.Extensions.DependencyInjection.Interfaces;

public interface IConfigureEventStoreEncryptionStage<T> where T : AggregateRoot
{
    public IConfigureEventStoreOptionsStage<T> Options(EventRepositoryOptions eventRepositoryOptions);
    public IConfigureEventStoreMetadataEnricherStage<T> MetadataEnricher(IEventMetadataEnricher eventMetadataEnricher);
    public IConfigureEventStoreSerializerStage<T> Serializer(IAggregateRootSerializer<T> aggregateRootSerializer);
    public IEventRepository<T> Build();
}
