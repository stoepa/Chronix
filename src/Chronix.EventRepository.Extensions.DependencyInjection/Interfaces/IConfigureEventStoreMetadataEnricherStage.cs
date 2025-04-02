using Chronix.EventRepository.Interfaces;
using Chronix.EventRepository.Options;
using Chronix.EventRepository.Types;

namespace Chronix.EventRepository.Extensions.DependencyInjection.Interfaces;

public interface IConfigureEventStoreMetadataEnricherStage<T> where T : AggregateRoot
{
    public IConfigureEventStoreOptionsStage<T> Options(EventRepositoryOptions eventRepositoryOptions);
    public IConfigureEventStoreEncryptionStage<T> Encryption(IEventDataEncryptor eventDataEncryptor);
    public IConfigureEventStoreSerializerStage<T> Serializer(IAggregateRootSerializer<T> aggregateRootSerializer);
    public IEventRepository<T> Build();
}
