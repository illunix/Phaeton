using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phaeton.Auth;
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

        switch (options.NodeType)
        {
            case "server":
                services.AddGrpc(q =>
                {
                    q.Interceptors.Add<ServerLoggerInterceptor>();
                });
                break;
            case "client":
                break;
        }
        services.Configure<AuthOptions>(section);
        
        return services;
    }
}
