using Microsoft.Extensions.DependencyInjection;
using Phaeton.Contexts.Accessors;
using Phaeton.Contexts.Providers;

namespace Phaeton.Contexts;

public static class Extensions
{
    public static IServiceCollection AddContexts(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor()
            .AddSingleton<IContextProvider, ContextProvider>()
            .AddSingleton<IContextAccessor, ContextAccessor>();

        return services;
    }
}