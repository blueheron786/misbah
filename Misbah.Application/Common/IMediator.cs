using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Application.Common
{
    /// <summary>
    /// Base interface for all commands
    /// </summary>
    public interface ICommand
    {
    }
    
    /// <summary>
    /// Base interface for commands with return values
    /// </summary>
    public interface ICommand<out TResult>
    {
    }
    
    /// <summary>
    /// Base interface for all queries
    /// </summary>
    public interface IQuery<out TResult>
    {
    }
    
    /// <summary>
    /// Handler interface for commands
    /// </summary>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Handler interface for commands with return values
    /// </summary>
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Handler interface for queries
    /// </summary>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Mediator pattern interface for handling commands and queries
    /// </summary>
    public interface IMediator
    {
        Task SendAsync(ICommand command, CancellationToken cancellationToken = default);
        Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }
}
