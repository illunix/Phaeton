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
 @$"namespace Phaeton.gRPC;

public static partial class Extensions
{{
    public static IApplicationBuilder MapgRPCServices(this IApplicationBuilder app)
    {{
        Console.WriteLine(""elo"");

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