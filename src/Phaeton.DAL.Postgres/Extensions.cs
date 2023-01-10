using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phaeton.DAL.Postgres.Abstractions;

namespace Phaeton.DAL.Postgres;

public static class Extensions
{
    public static IServiceCollection AddPostgres<T, K>(
        this IServiceCollection services,
        IConfiguration configuration
    )
        where T : IDataContext
        where K : DbContext, T
    {
        var section = configuration.GetSection("postgres");
        if (!section.Exists())
            return services;

        var options = section.BindOptions<PostgresOptions>();

        services.Configure<PostgresOptions>(section);
        services.AddDbContext<T, K>(q =>
        {
            q.UseNpgsql(options.ConnectionString);
            q.ConfigureWarnings(warnings =>
            {
                warnings.Default(WarningBehavior.Log);
                // ignore the RowLimitingOperation warning to prevent this warning
                // at firstordefault or other linq-to-sql operations
                warnings.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning);
            });
        });

        return services;
    }
}