namespace Phaeton.Abstractions;

public interface IQueryHandler<in T, K> where T : class, IQuery<K>
{
    Task<K> Handle(
        T req,
        CancellationToken ct = default
    );
}