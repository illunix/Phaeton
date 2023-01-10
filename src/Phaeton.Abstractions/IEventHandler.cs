namespace Tesseract.Abstractions;

public interface IEventHandler<in T> where T :
    class,
    IEvent
{
    Task Handle(
        T @event,
        CancellationToken ct = default
    );
}