using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Domain.Interfaces
{
    /// <summary>
    /// Interface for handling domain events
    /// </summary>
    public interface IDomainEventHandler<in T> where T : Events.DomainEvent
    {
        Task HandleAsync(T domainEvent, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Interface for dispatching domain events
    /// </summary>
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : Events.DomainEvent;
        Task DispatchAsync(IEnumerable<Events.DomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
