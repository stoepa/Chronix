using Chronix.EventRepository.Encryptors;
using Chronix.EventRepository.Enrichers;
using Chronix.EventRepository.Extensions.DependencyInjection.Interfaces;
using Chronix.EventRepository.Interfaces;
using Chronix.EventRepository.Options;
using Chronix.EventRepository.Repositories;
using Chronix.EventRepository.Serializers;
using Chronix.EventRepository.Types;
using EventStore.Client;


namespace Chronix.EventRepository.Extensions.DependencyInjection;

public class EventRepositoryBuilder<T> :
    IConfigureEventStoreClientStage<T>,
    IConfigureEventStoreOptionsStage<T>,
    IConfigureEventStoreEncryptionStage<T>,
    IConfigureEventStoreMetadataEnricherStage<T>,
    IConfigureEventStoreSerializerStage<T> where T : AggregateRoot
{
    private readonly string streamName;
    private readonly EventStoreClient eventStoreClient;
    private EventRepositoryOptions? eventRepositoryOptions;
    private IAggregateRootSerializer<T>? aggregateRootSerializer;
    private IEventDataEncryptor? eventDataEncryptor;
    private IEventMetadataEnricher? eventMetadataEnricher;
    private readonly IServiceProvider services;

    private EventRepositoryBuilder(EventStoreClient eventStoreClient, string streamName, IServiceProvider serviceProvider)
    {
        this.eventStoreClient = eventStoreClient;
        this.streamName = streamName;
		services = serviceProvider;
    }

    public static IConfigureEventStoreClientStage<T> Create(EventStoreClient eventStoreClient, string streamName, IServiceProvider serviceProvider)
    {
        return new EventRepositoryBuilder<T>(eventStoreClient, streamName, serviceProvider);
    }

    public IConfigureEventStoreEncryptionStage<T> Encryption(IEventDataEncryptor eventDataEncryptor)
    {
        if (eventDataEncryptor != null)
            this.eventDataEncryptor = eventDataEncryptor;

        return this;
    }

    public IConfigureEventStoreMetadataEnricherStage<T> MetadataEnricher(IEventMetadataEnricher eventMetadataEnricher)
    {
        if (eventMetadataEnricher != null)
            this.eventMetadataEnricher = eventMetadataEnricher;

        return this;
    }

    public IConfigureEventStoreOptionsStage<T> Options(EventRepositoryOptions eventRepositoryOptions)
    {
        if (eventRepositoryOptions != null)
            this.eventRepositoryOptions = eventRepositoryOptions;

        return this;
    }

    public IConfigureEventStoreSerializerStage<T> Serializer(IAggregateRootSerializer<T> aggregateRootSerializer)
    {
        if (aggregateRootSerializer != null)
            this.aggregateRootSerializer = aggregateRootSerializer;

        return this;
    }

    public IEventRepository<T> Build()
    {
        eventRepositoryOptions ??= new EventRepositoryOptions();
        eventDataEncryptor ??= new NoEncryptionEncrypter();
        eventMetadataEnricher ??= new BasicMetadataEnricher();
        aggregateRootSerializer ??= new AggregateRootSerializer<T>(eventDataEncryptor, eventMetadataEnricher);

        return new EventRepository<T>(eventStoreClient, streamName,
			aggregateRootSerializer, eventRepositoryOptions, services);
    }
}
