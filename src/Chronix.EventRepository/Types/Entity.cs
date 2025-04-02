using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Chronix.EventRepository.Types;

public abstract class Entity
{

    [JsonProperty]
    public Guid Id { get; protected set; }
    [IgnoreDataMember]
    protected AggregateRoot? AggregateRoot;

    protected Entity()
    {
    }

    protected Entity(AggregateRoot aggregateRoot)
    {
        AggregateRoot = aggregateRoot;
    }

    protected void Raise(DomainEvent domainEvent)
    {
        if (AggregateRoot == null)
            throw new Exception("AggregateRoot is null, please call the SetAggregateRoot function on any entities. Also remember to do this on rehydration.");
        AggregateRoot.Raise(domainEvent);
    }

    protected void SetAggregateRoot(AggregateRoot aggregateRoot)
    {
        AggregateRoot ??= aggregateRoot;
    }
}
