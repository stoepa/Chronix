using Chronix.EventRepository.Tools;
using Chronix.EventRepository.Types;

namespace Chronix.EventRepository.Interfaces;

public interface IEventRepository<T> where T : AggregateRoot
{
    Task<Result<T>> GetById(string id, CancellationToken cancellationToken = default);
    Task<Result<long>> Append(T aggregate, CancellationToken cancellationToken = default, params IProjection[] projections);
    Task<Result<bool>> Exists(string id, CancellationToken cancellationToken = default);
}
