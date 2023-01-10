using Microsoft.Extensions.DependencyInjection;
using Phaeton.Abstractions;

namespace Phaeton.Dispatchers;

internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task<T> Query<T>(
        IQuery<T> req,
        CancellationToken ct = default
    )
    {
        if (req is null)
            throw new InvalidOperationException("Query cannot be null.");

        await using var scope = _serviceProvider.CreateAsyncScope();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(
            req.GetType(),
            typeof(T)
        );
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<T>, T>.Handle));
        if (method is null)
            throw new InvalidOperationException($"Query handler for '{typeof(T).Name}' is invalid.");

#pragma warning disable CS8602
#pragma warning disable CS8600
        return await (Task<T>)method.Invoke(
            handler,
            new object[]
            {
                req,
                ct
            }
        );
#pragma warning restore CS8600
#pragma warning restore CS8602
    }
}