using Chronix.EventRepository.Interfaces;
using Chronix.EventRepository.Options;
using Chronix.EventRepository.Types;

namespace Chronix.EventRepository.Extensions.DependencyInjection.Interfaces;

public interface IConfigureEventStoreOptionsStage<T> where T : AggregateRoot
{
    public IConfigureEventStoreEncryptionStage<T> Encryption(IEventDataEncryptor eventDataEncryptor);
    public IConfigureEventStoreMetadataEnricherStage<T> MetadataEnricher(IEventMetadataEnricher eventMetadataEnricher);
    public IConfigureEventStoreSerializerStage<T> Serializer(IAggregateRootSerializer<T> aggregateRootSerializer);
    public IEventRepository<T> Build();
}
