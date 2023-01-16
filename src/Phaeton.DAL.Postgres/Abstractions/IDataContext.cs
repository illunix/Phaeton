namespace Phaeton.DAL.Postgres.Abstractions;

public interface IDataContext : IDisposable
{
    Task<int> SaveChanges();
}