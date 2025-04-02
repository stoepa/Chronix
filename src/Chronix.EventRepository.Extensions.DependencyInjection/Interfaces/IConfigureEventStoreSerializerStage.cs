using Chronix.EventRepository.Interfaces;
using Chronix.EventRepository.Options;
using Chronix.EventRepository.Types;

namespace Chronix.EventRepository.Extensions.DependencyInjection.Interfaces;

public interface IConfigureEventStoreSerializerStage<T> where T : AggregateRoot
{
    public IConfigureEventStoreOptionsStage<T> Options(EventRepositoryOptions eventRepositoryOptions);
    public IConfigureEventStoreEncryptionStage<T> Encryption(IEventDataEncryptor eventDataEncryptor);
    public IConfigureEventStoreMetadataEnricherStage<T> MetadataEnricher(IEventMetadataEnricher eventMetadataEnricher);
    public IEventRepository<T> Build();
}
