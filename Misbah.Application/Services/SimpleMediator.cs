using Microsoft.Extensions.DependencyInjection;
using Misbah.Application.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Application.Services
{
    /// <summary>
    /// Simple mediator implementation for handling commands and queries
    /// </summary>
    public class SimpleMediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        
        public SimpleMediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);
            await handler.HandleAsync((dynamic)command, cancellationToken);
        }
        
        public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);
            return await handler.HandleAsync((dynamic)command, cancellationToken);
        }
        
        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);
            return await handler.HandleAsync((dynamic)query, cancellationToken);
        }
    }
}
