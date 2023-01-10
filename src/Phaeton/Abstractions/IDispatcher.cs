namespace Phaeton.Abstractions;

public interface IDispatcher
{
    Task Send<T>(
        T req,
        CancellationToken ct = default
    ) where T :
        class,
        ICommand;
    Task<T> Send<T>(
        IQuery<T> req,
        CancellationToken ct = default
    );
    Task Publish<T>(
        T @event,
        CancellationToken ct = default
    ) where T :
        class,
        IEvent;
}