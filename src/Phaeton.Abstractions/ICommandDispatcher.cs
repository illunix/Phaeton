namespace Tesseract.Abstractions;

public interface ICommandDispatcher
{
    Task Send<T>(
        T req,
        CancellationToken ct = default
    ) where T :
        class,
        ICommand;
}