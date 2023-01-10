namespace Tesseract.Abstractions;

public interface ICommandHandler<in T> where T :
    class,
    ICommand
{
    Task Handle(
        T req,
        CancellationToken ct = default
    );
}