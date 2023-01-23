using Microsoft.EntityFrameworkCore;
using Phaeton.DAL.Postgres.DbContext.Generator.Sample.Entities;

namespace Phaeton.DAL.Postgres.DbContext.Generator.Sample.Context;

[DbContext]
public sealed partial class UsersContext
{
    public DbSet<UserEntity>? Users { get; init; }
}