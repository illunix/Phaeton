using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Phaeton.Generator.Extensions;

public static class Extensions
{
    public static IEnumerable<INamedTypeSymbol> GetSymbolsWithAttribute<T>(
        this T syntaxReceiver,
        GeneratorExecutionContext ctx,
        string attrName
    ) where T : ISyntaxReceiver
        => ((IEnumerable<ClassDeclarationSyntax>)syntaxReceiver
            .GetType()
            .GetProperty("CandidateClasses")
            .GetValue(
                syntaxReceiver,
                null
            ))
            .Select(q =>
                (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(q.SyntaxTree)
                    .GetDeclaredSymbol(q)!
            )
            .TakeWhile(q => q is not null)
            .Where(q => q
                .GetAttributes()
                .Any(q => q.AttributeClass?.Name == attrName)
            )
            .ToList();

    public static string? GetNamespaceName(this INamedTypeSymbol symbol)
        => symbol.ContainingNamespace.ToDisplayString();

    public static AttributeData? GetAttributeByName(
        this INamedTypeSymbol symbol,
        string attrName
    )
        => symbol.GetAttributes().FirstOrDefault(q => q.AttributeClass?.Name == attrName);

    public static object? GetAttributeFirstConstructorArgValue(this AttributeData attr)
        => attr.ConstructorArguments
            .Select(q => q.Value)
            .FirstOrDefault();

    public static IEnumerable<ISymbol> GetMembersWithoutCtor(this INamedTypeSymbol symbol)
    {
        var members = symbol.GetMembers();

        members = members.Remove(members
            .Where(q => q.Name == ".ctor")
            .FirstOrDefault()
        );

        return members;
    }
}