using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Chronix.EventRepository.Types;

public abstract class AggregateRoot
{
    private protected List<EventHandler> handlers = [];
    private protected List<DomainEventData> raised = [];
    private protected List<string> allStreamNames = [];

    [JsonProperty]
    public Guid Id { get; protected set; }

    [DataMember]
    public long Version { get; protected set; } = -1L;
    [DataMember]
    public long CurrentStreamVersion { get; protected set; } = -1L;

    public abstract string Revision();

    public void Raise(DomainEvent domainEvent) => Raise(domainEvent, true);

    public IReadOnlyCollection<DomainEventData> GetChanges() => raised.AsReadOnly();

    public void ClearRaisedEvents() => raised.Clear();

    public void Load(List<DomainEvent> events, List<string> lastStreamName, long version, long currentStreamVersion)
    {
        foreach (var ev in events)
            Raise(ev, false);

        if (lastStreamName != null)
        {
            foreach (var streamName in allStreamNames)
                allStreamNames.Add(streamName);
        }
        Version = version;
        CurrentStreamVersion = currentStreamVersion;
    }

    protected void RegisterHandler<T>(Action<T> when) where T : DomainEvent
    {
        handlers.Add(new EventHandler(typeof(T), delegate (DomainEvent e)
        {
            when((T)e);
        }));
    }

    private void Raise(DomainEvent domainEvent, bool isNew = true)
    {
        var handler = handlers.FirstOrDefault(h => h.Type == domainEvent.GetType()) ?? throw new Exception("When register a event handler with metadata, you need to explicitly create the metadata and send it in on the raise");
        handler.When(domainEvent);

        if (isNew)
        {
            var domainEventMetadata = new DomainEventMetadata(Id, DateTime.UtcNow, []);

            raised.Add(new DomainEventData(domainEvent, domainEventMetadata));
        }
    }
}
