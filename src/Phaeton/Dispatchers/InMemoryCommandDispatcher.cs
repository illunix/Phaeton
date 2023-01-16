using Microsoft.Extensions.DependencyInjection;
using Phaeton.Abstractions;

namespace Phaeton.Dispatchers;

internal sealed class InMemoryCommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public InMemoryCommandDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task Send<T>(
        T req,
        CancellationToken ct = default
    ) where T :
        class,
        ICommand
    {
        if (req is null)
            throw new InvalidOperationException("Command cannot be null.");

        await using var scope = _serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>();

        await handler.Handle(
            req,
            ct
        );
    }
}