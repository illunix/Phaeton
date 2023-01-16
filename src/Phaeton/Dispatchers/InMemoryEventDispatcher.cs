using Microsoft.Extensions.DependencyInjection;
using Phaeton.Abstractions;

namespace Phaeton.Dispatchers;

internal sealed class InMemoryEventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public InMemoryEventDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task Publish<T>(
        T @event,
        CancellationToken ct = default
    ) where T :
        class,
        IEvent
    {
        if (@event is null)
            throw new InvalidOperationException("Event cannot be null.");

        await using var scope = _serviceProvider.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();

        var tasks = handlers.Select(q => q.Handle(
            @event, ct
        ));

        await Task.WhenAll(tasks);
    }
}