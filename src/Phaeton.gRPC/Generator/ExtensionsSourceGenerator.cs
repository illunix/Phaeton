using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;

namespace Phaeton.gRPC.Server.Extensions.Generator;

[Generator]
internal sealed class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {

        if (ctx.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            return;

        var sb = new StringBuilder();

        var classes = new List<INamedTypeSymbol>();

        foreach (var @class in syntaxReceiver.CandidateClasses.Select(classSyntax => (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax)!).TakeWhile(@class => @class is not null))
        {
            if (@class.GetAttributes().Any(q => q.AttributeClass?.Name == nameof(gRPCServiceAttribute)))
                classes.Add(@class);

            sb.AppendLine(
 @$"
using Microsoft.Extensions.Options;
using Grpc.Net.Client;

namespace Phaeton.gRPC;

public static partial class Extensions
{{
    public static IApplicationBuilder UsegRPC(this IApplicationBuilder app)
    {{
        var options = app.ApplicationServices.GetRequiredService<IOptions<gRPCOptions>>().Value;
        if (!options.Enabled)
            return app;

        using var channel = GrpcChannel.ForAddress(options.Url);

        return app;
    }}
}}
                "
            );
        }

        ctx.AddSource(
            "Phaeton.gRPC.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }
}