using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Phaeton.gRPC.Server.Extensions.Generator;

namespace Phaeton.gRPC.Server.Extensions.Generator;

[Generator]
internal sealed class gRPCExtensionsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {
        if (ctx.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            return;

        var sb = new StringBuilder();

        var classes = syntaxReceiver.CandidateClasses
            .Select(classSyntax =>
                (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(classSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classSyntax)!).TakeWhile(@class => @class is not null).Where(@class =>
                @class.GetAttributes().Any(q => q.AttributeClass?.Name == nameof(gRPCServiceAttribute))).ToList();

        foreach (var @class in classes)
        {
            sb.AppendLine(
@$"using Microsoft.Extensions.Options;
using Grpc.Net.ClientFactory;
using Phaeton.gRPC.Interceptors;

namespace Phaeton.gRPC;

public static class Extensions
{{
    public static IApplicationBuilder MapgRPCServices(this WebApplication app)
    {{
        var options = app.Services.GetRequiredService<IOptions<gRPCOptions>>().Value;
        /*
        if (!options.Enabled)
            throw new ArgumentException(nameof(options.Enabled));
        */

        app
            .MapGrpcService<{@class}>();

        return app;
    }}

    public static IServiceCollection AddgRPCClients(this IServiceCollection services)
    {{
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<gRPCOptions>>().Value;
        /*
        if (!options.Enabled)
            throw new ArgumentException(nameof(options.Enabled));
        */

        services.AddGrpcClient<{@class.BaseType}>().AddInterceptor<ClientLoggerInterceptor>(InterceptorScope.Client);

        return services;
    }}
}}
"
            );
        }

        ctx.AddSource(
            "Phaeton.gRPC.Server.Extensions.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }
}