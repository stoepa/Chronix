using Chronix.EventRepository.Types;

namespace Chronix.EventRepository.Interfaces;

public interface IProjection
{
    public string EventType { get; }
    public Task Project(AggregateRoot aggregateRoot, DomainEvent eventToProject, CancellationToken cancellationToken);

    public Task Rollback(AggregateRoot aggregateRoot, DomainEvent eventToRollback, CancellationToken cancellationToken, Exception? exception = null);
}

public interface IProjection<TAggregate, TEvent> : IProjection
    where TAggregate : AggregateRoot
    where TEvent : DomainEvent
{
    Task Project(TAggregate aggregateRoot, TEvent @event, CancellationToken cancellationToken);
    Task Rollback(TAggregate aggregateRoot, TEvent @event, CancellationToken cancellationToken, Exception? exception = null);
}

public abstract class ProjectionBase<TAggregate, TEvent> : IProjection<TAggregate, TEvent>
    where TAggregate : AggregateRoot
    where TEvent : DomainEvent
{
    public string EventType => typeof(TEvent).Name;

    public abstract Task Project(TAggregate aggregateRoot, TEvent @event, CancellationToken cancellationToken);
    public abstract Task Rollback(TAggregate aggregateRoot, TEvent @event, CancellationToken cancellationToken, Exception? exception = null);

    async Task IProjection.Project(AggregateRoot aggregateRoot, DomainEvent @event, CancellationToken cancellationToken)
    {
        await Project((TAggregate)aggregateRoot, (TEvent)@event, cancellationToken);
    }

    async Task IProjection.Rollback(AggregateRoot aggregateRoot, DomainEvent @event, CancellationToken cancellationToken, Exception? exception)
    {
        await Rollback((TAggregate)aggregateRoot, (TEvent)@event, cancellationToken, exception);
    }
}
