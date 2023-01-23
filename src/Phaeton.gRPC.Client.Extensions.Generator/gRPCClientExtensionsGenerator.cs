using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Phaeton.gRPC.Server.Extensions.Generator;

namespace Phaeton.gRPC.Client.Extensions.Generator;

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
                @class.GetMembers().Any(q => q.Name == "__ServiceName")).ToList();
        if (!classes.Any())
            return;

        foreach (var @class in classes)
        {
            sb.AppendLine(
@$"using Microsoft.Extensions.Options;
using Grpc.Net.Client;

namespace Phaeton.gRPC;

public static class Extensions
{{
    public static IServiceCollection AddgRPCClients(this IServiceCollection services)
    {{
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<gRPCOptions>>().Value;
        /*
        if (!options.Enabled)
            throw new ArgumentException(nameof(options.Enabled));
        */

        using var channel = GrpcChannel.ForAddress(""https://localhost:7044"");

        var client = new {@class}.{@class.Name}Client(channel);
            
        services.AddSingleton(client);

        return services;
    }}
}}
"
            );
        }

        ctx.AddSource(
            "Phaeton.gRPC.Client.Extensions.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }
}