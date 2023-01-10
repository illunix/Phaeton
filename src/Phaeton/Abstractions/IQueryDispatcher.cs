namespace Phaeton.Abstractions;

public interface IQueryDispatcher
{
    Task<T> Query<T>(
        IQuery<T> query,
        CancellationToken ct = default
    );
}