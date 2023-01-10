using Microsoft.EntityFrameworkCore;

namespace Phaeton.DAL.Postgres.Abstractions;

public abstract class DbDataContext : DbContext, IDataContext
{
    protected DbDataContext(DbContextOptions options) : base(options) { }

    public async Task<int> SaveChangesAsync()
        => await base.SaveChangesAsync();
}