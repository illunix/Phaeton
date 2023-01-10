namespace Tesseract.Abstractions;

public interface IEventDispatcher
{
    Task Publish<T>(
        T @event,
        CancellationToken cancellationToken = default
    ) where T :
        class,
        IEvent;
}