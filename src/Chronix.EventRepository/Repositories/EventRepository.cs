using Chronix.EventRepository.Interfaces;
using Chronix.EventRepository.Options;
using Chronix.EventRepository.Tools;
using Chronix.EventRepository.Types;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text;

namespace Chronix.EventRepository.Repositories;

public class EventRepository<T>(EventStoreClient eventStoreClient, string streamBaseName,
    IAggregateRootSerializer<T> eventDataSerializerFunc, EventRepositoryOptions eventRepositoryOptions, IServiceProvider serviceProvider) : IEventRepository<T> where T : AggregateRoot
{
    private readonly string streamBaseName = $"{streamBaseName.TrimEnd('-')}-";

    public async Task<Result<long>> Append(T aggregate, CancellationToken cancellationToken = default, params IProjection[] projections)
    {
        using (var scope = serviceProvider.CreateScope())
        {

            if (projections == null || projections.Length == 0)
            {
                var assemblyProjections = scope.ServiceProvider.GetServices<IProjection>();

                if (assemblyProjections != null)
                    projections = assemblyProjections.ToArray();
                else
                    projections = [];
            }

            var result = await AppendInternal(aggregate, [.. projections], cancellationToken);

            if (result.IsSuccess)
            {
                if (eventRepositoryOptions.AutoRevisionAfterNthEvent != -1 && result.Value > eventRepositoryOptions.AutoRevisionAfterNthEvent)
                {
                    // Versioning
                    aggregate.ClearRaisedEvents();

                    var json = aggregate.Revision();

                    await VersionStream(aggregate, json, cancellationToken);
                }
            }

            return result;
        }
    }

    private async Task VersionStream(T aggregate, string json, CancellationToken cancellationToken)
    {
        var streamName = streamBaseName + aggregate.Id;
        var newStreamId = Guid.NewGuid();
        var newStreamName = streamBaseName + newStreamId;
        var lastStream = await FindRevision(streamName, cancellationToken);
        var streamRevisionEvent = new StreamRevisionEvent(newStreamName,
            streamName, DateTime.Now, lastStream.Item2 + 1);
        var streamRevisionSnapshotEvent = new StreamRevisionSnapshotEvent(lastStream.Item1.Last(),
            streamName, DateTime.Now, lastStream.Item2 + 2, json);

        var eventDataNew = new EventData(Uuid.NewUuid(), nameof(StreamRevisionSnapshotEvent), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(streamRevisionSnapshotEvent)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new DomainEventMetadata(aggregate.Id, DateTime.UtcNow, null))));

        var newStreamResult = eventStoreClient.AppendToStreamAsync(newStreamName, StreamState.NoStream, [eventDataNew], cancellationToken: cancellationToken);

        var eventData = new EventData(Uuid.NewUuid(), nameof(StreamRevisionEvent), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(streamRevisionEvent)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new DomainEventMetadata(AggregateId: aggregate.Id, DateTime.UtcNow, null))));

        var oldStreamResult = await eventStoreClient.AppendToStreamAsync(lastStream.Item1.Last(), StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);
    }

    private async Task<Result<long>> AppendInternal(T aggregate, List<IProjection> projections, CancellationToken cancellationToken = default)
    {
        var eventsData = new List<EventData>();
        var projected = new Dictionary<DomainEvent, List<IProjection>>();
        foreach (var ev in aggregate.GetChanges())
        {
            eventsData.Add(eventDataSerializerFunc.Serialize(ev.DomainEvent, ev.DomainEventMetadata));
            var eventProjections = projections.Where(p => p.EventType == ev.DomainEvent.EventType);
            foreach (var projection in eventProjections)
            {
                try
                {
                    await projection.Project(aggregate, ev.DomainEvent, cancellationToken);
                    if (!projected.ContainsKey(ev.DomainEvent))
                        projected[ev.DomainEvent] = [];

                    projected[ev.DomainEvent].Add(projection);
                }
                catch (Exception ex)
                {
                    foreach (var proj in projected)
                        foreach (var p in proj.Value)
                            await p.Rollback(aggregate, proj.Key, cancellationToken, ex);

                    return Result<long>.Failure(ex.Message);
                }
            }
        }

        var streamName = streamBaseName + aggregate.Id;
        if (aggregate.Version == -1L)
        {
            try
            {
                var result = await eventStoreClient.AppendToStreamAsync(streamName, StreamState.NoStream, eventsData, cancellationToken: cancellationToken);
                if (result is WrongExpectedVersionResult)
                {
                    foreach (var proj in projected)
                        foreach (var p in proj.Value)
                            await p.Rollback(aggregate, proj.Key, cancellationToken);
                    return Result<long>.Failure("Wrong Expected Version");
                }
                return Result<long>.Success(result.NextExpectedStreamRevision.ToInt64());
            }
            catch (Exception ex)
            {
                foreach (var proj in projected)
                    foreach (var p in proj.Value)
                        await p.Rollback(aggregate, proj.Key, cancellationToken, ex);
                return Result<long>.Failure("Wrong Expected Version");
            }
        }
        else
        {
            try
            {
                var result = await eventStoreClient.AppendToStreamAsync(streamName, Convert.ToUInt64(aggregate.CurrentStreamVersion), eventsData, cancellationToken: cancellationToken);
                if (result is WrongExpectedVersionResult)
                {
                    foreach (var proj in projected)
                        foreach (var p in proj.Value)
                            await p.Rollback(aggregate, proj.Key, cancellationToken);
                    return Result<long>.Failure("Wrong Expected Version");
                }
                return Result<long>.Success(result.NextExpectedStreamRevision.ToInt64());
            }
            catch (Exception ex)
            {
                foreach (var proj in projected)
                    foreach (var p in proj.Value)
                        await p.Rollback(aggregate, proj.Key, cancellationToken, ex);
                return Result<long>.Failure("Wrong Expected Version");
            }
            
        }
    }

    public async Task<Result<bool>> Exists(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var events = eventStoreClient.ReadStreamAsync(Direction.Forwards, streamBaseName + id, StreamPosition.Start, cancellationToken: cancellationToken);
            if (await events.ReadState == ReadState.Ok)
                return Result<bool>.Success(true);
            return Result<bool>.Success(false);
        }
        catch (Exception e)
        {
            return Result<bool>.Failure(e.Message, e.InnerException != null ? e.InnerException.Message : string.Empty);
        }
    }

    public async Task<Result<T>> GetById(string id, CancellationToken cancellationToken = default)
    {
        var streamName = streamBaseName + id;
        Tuple<List<string>, long>? revisions = null;
        var version = -1L;
        revisions = await FindRevision(streamName, cancellationToken);
        streamName = revisions.Item1.Last();

        var events = eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, cancellationToken: cancellationToken);
        if (events.ReadState.Result == ReadState.StreamNotFound)
            return Result<T>.Failure("Stream not found");

        var domainEvents = new List<DomainEvent>();
        await foreach (var ev in events)
        {
            var domainEvent = eventDataSerializerFunc.Deserializer(ev.Event.EventType, ev.Event.Data.Span);
            version++;
            if (ev.Event.EventType == nameof(StreamRevisionSnapshotEvent))
            {
                var srse = (StreamRevisionSnapshotEvent)domainEvent;
                version = srse.Version;
            }
            domainEvents.Add(domainEvent);
        }

        try
        {
            var aggregate = (T?)Activator.CreateInstance(typeof(T), true);

            if (aggregate == null)
                return Result<T>.Failure("Failed to create instance of aggregate");

            aggregate.Load(domainEvents, revisions.Item1, version, revisions.Item2);

            return Result<T>.Success(aggregate);
        }
        catch (Exception)
        {
            return Result<T>.Failure("Failed to create instance of aggregate");
        }

    }

    private async Task<Tuple<List<string>, long>> FindRevision(string streamName, CancellationToken cancellationToken, long version = -1L, List<string>? previousNames = null)
    {
        previousNames ??= [];
        previousNames.Add(streamName);

        var last = eventStoreClient.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1, cancellationToken: cancellationToken);
        if (last != null)
        {
            try
            {
                await foreach (var ev in last)
                {
                    if (ev.Event.EventType == nameof(StreamRevisionEvent))
                    {
                        var newStreamName = JsonConvert.DeserializeObject<StreamRevisionEvent>(Encoding.UTF8.GetString(ev.Event.Data.Span));
                        if (newStreamName != null)
                            return await FindRevision(newStreamName.NextStreamName, cancellationToken, newStreamName.Version, previousNames);
                        else
                            throw new Exception("Could not deserialize last StreamRevisionEvent");
                    }
                    else
                    {
                        var lastStream = eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, cancellationToken: cancellationToken);
                        await foreach (var _ in lastStream)
                            version++;
                        return new Tuple<List<string>, long>(previousNames, version);
                    }
                }
            }
            catch (Exception)
            {
            }

        }
        return new Tuple<List<string>, long>(previousNames, 0L);
    }
}
