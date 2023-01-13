using System.Diagnostics;
using Grpc.Net.Client;
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
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
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
            {
                foreach (var address in options.Addresses)
                {
                    switch (address)
                    {
                        case null:
                            throw new ArgumentNullException(nameof(address));
                        default:
                            if (string.IsNullOrEmpty(address))
                                throw new ArgumentException(nameof(address));
                            break;
                    }
                }
                break;
            }
        }
        services.Configure<AuthOptions>(section);
        
        return services;
    }
}
