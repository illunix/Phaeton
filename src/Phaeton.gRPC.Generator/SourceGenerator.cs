using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Phaeton.gRPC.Generator;

[Generator]
internal sealed class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {
#if DEBUGATTACH
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        if (ctx.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            return;

        var classes = new List<INamedTypeSymbol>();

        foreach (var @class in syntaxReceiver.CandidateClasses)
        {
            var classSymbol = (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(@class.SyntaxTree).GetDeclaredSymbol(@class)!;
            if (classSymbol is null)
                break;

            if (classSymbol.GetAttributes().Any(q => q.AttributeClass?.Name == nameof(gRPCServiceAttribute)))
                classes.Add(classSymbol);
        }

        var sb = new StringBuilder();

        sb.AppendLine(
            @"using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Authorization;
            using DispatchEndpoints;"
        );

        ctx.AddSource(
            "Phaeton.gRPC.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }
}