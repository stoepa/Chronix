using Chronix.EventRepository.Interfaces;
using Chronix.EventRepository.Types;
using EventStore.Client;
using Newtonsoft.Json;
using System.Text;

namespace Chronix.EventRepository.Serializers;

public class AggregateRootSerializer<T> : IAggregateRootSerializer<T> where T : AggregateRoot
{
    private readonly IEventDataEncryptor eventDataEncryptor;
    private readonly IEventMetadataEnricher eventMetadataEnricher;
    private readonly Type[] assemblyTypes;

    public AggregateRootSerializer(IEventDataEncryptor eventDataEncryptor,IEventMetadataEnricher eventMetadataEnricher)
    {
        this.eventDataEncryptor = eventDataEncryptor;
        this.eventMetadataEnricher = eventMetadataEnricher;
        var asm = typeof(T).Assembly;
        assemblyTypes = asm.GetTypes();
    }

    public EventData Serialize(DomainEvent domainEvent, IDomainEventMetadata domainEventMetadata)
    {
        domainEventMetadata = eventMetadataEnricher.Enrich(domainEventMetadata);
        var eventMetaDataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(domainEventMetadata));

        var type = assemblyTypes.First(t => t.Name == domainEvent.EventType);
        byte[] eventDataBytes;

        eventDataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Convert.ChangeType(eventDataEncryptor.Encrypt(domainEvent), type)));

        return new EventData(Uuid.NewUuid(), domainEvent.EventType, eventDataBytes, eventMetaDataBytes);
    }

    public DomainEvent Deserializer(string eventName, ReadOnlySpan<byte> eventSpan)
    {
        var type = assemblyTypes.First(t => t.Name == eventName);
        var domainEvent = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(eventSpan), type) as DomainEvent ?? throw new Exception("Failed to deserialize domain event");
        return eventDataEncryptor.Decrypt(domainEvent);
    }
}
