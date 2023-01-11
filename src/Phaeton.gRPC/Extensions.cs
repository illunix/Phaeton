using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phaeton.Auth;
using Grpc.Core.Interceptors;
using Phaeton.gRPC.Interceptors;

namespace Phaeton.gRPC;

public static partial class Extensions
{
    public static IServiceCollection AddgRPC(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection("grpc");
        if (!section.Exists())
            return services;

        var options = section.BindOptions<gRPCOptions>();
        if (!options.Enabled)
            return services;

        services.Configure<AuthOptions>(section);

        services.AddGrpc(q =>
        {
            q.Interceptors.Add<ServerLoggerInterceptor>();
        });

        return services;
    }

    public static IApplicationBuilder MapGrpcServices(this IApplicationBuilder app)
    {
        return app;
    }
}

