using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Phaeton.gRPC.Server.Extensions.Generator;

namespace Phaeton.gRPC.Extensions.Generator;

[Generator]
internal sealed class gRPCExtensionsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public static List<INamedTypeSymbol> _classes { get; set; }
    
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
            _classes.Add(@class);
            
            sb.AppendLine(
@$"namespace Phaeton.gRPC;

public static class Extensions
{{
    public static IApplicationBuilder MapgRPCServices(this WebApplication app)
    {{
        app
            .MapGrpcService<{@class}>();

        return app;
    }}

    public static IServiceCollection AddgRPCClients(this IServiceCollection services)
    {{
        services.AddGrpcClient<{@class.BaseType}>(q => {{ }});

        return services;
    }}
}}
"


            );
        }

        ctx.AddSource(
            "Phaeton.gRPC.Extensions.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }
}