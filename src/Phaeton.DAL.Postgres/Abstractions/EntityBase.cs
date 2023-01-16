using System.Numerics;

namespace Phaeton.DAL.Postgres.Abstractions;

public class EntityBase<T> where  T : INumber<T>
{
    public T Id { get; init; }
}

public class EntityBase
{
    public Guid Id { get; init; }
}