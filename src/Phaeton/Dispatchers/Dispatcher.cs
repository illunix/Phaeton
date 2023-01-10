using Phaeton.Abstractions;

namespace Phaeton.Dispatchers;

internal sealed class Dispatcher : IDispatcher
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public Dispatcher(
        ICommandDispatcher commandDispatcher,
        IEventDispatcher eventDispatcher,
        IQueryDispatcher queryDispatcher
    )
    {
        _commandDispatcher = commandDispatcher;
        _eventDispatcher = eventDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    public Task Send<T>(
        T req,
        CancellationToken ct = default
    ) where T : class, ICommand
        => _commandDispatcher.Send(
            req,
            ct
        );

    public Task<TResult> Send<TResult>(
        IQuery<TResult> req,
        CancellationToken ct = default
    )
        => _queryDispatcher.Query(
            req,
            ct
        );

    public Task Publish<T>(
        T @event,
        CancellationToken ct = default
    ) where T :
        class,
        IEvent
        => _eventDispatcher.Publish(
            @event,
            ct
        );
}