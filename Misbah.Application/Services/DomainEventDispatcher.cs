using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Misbah.Domain.Events;
using Misbah.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Application.Services
{
    /// <summary>
    /// Simple domain event dispatcher implementation
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DomainEventDispatcher> _logger;
        
        public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        
        public async Task DispatchAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : DomainEvent
        {
            try
            {
                var handlers = _serviceProvider.GetServices<IDomainEventHandler<T>>();
                var tasks = handlers.Select(handler => handler.HandleAsync(domainEvent, cancellationToken));
                
                await Task.WhenAll(tasks);
                
                _logger.LogInformation("Successfully dispatched domain event {EventType} with ID {EventId}",
                    domainEvent.EventType, domainEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching domain event {EventType} with ID {EventId}",
                    domainEvent.EventType, domainEvent.Id);
                throw;
            }
        }
        
        public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            var eventList = domainEvents.ToList();
            
            if (!eventList.Any())
                return;
                
            _logger.LogDebug("Dispatching {EventCount} domain events", eventList.Count);
            
            var tasks = eventList.Select(async domainEvent =>
            {
                var eventType = domainEvent.GetType();
                var dispatchMethod = typeof(IDomainEventDispatcher)
                    .GetMethod(nameof(DispatchAsync), new[] { eventType, typeof(CancellationToken) });
                    
                if (dispatchMethod != null)
                {
                    var task = (Task)dispatchMethod.Invoke(this, new object[] { domainEvent, cancellationToken })!;
                    await task;
                }
            });
            
            await Task.WhenAll(tasks);
        }
    }
}
