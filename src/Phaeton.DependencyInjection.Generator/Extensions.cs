using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Phaeton.Generator.Extensions;

public static class Extensions
{
    public static IEnumerable<INamedTypeSymbol> GetSymbols<T>(
        this T syntaxReceiver,
        GeneratorExecutionContext ctx
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
            .ToList();

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
        => $"{symbol.ContainingNamespace};";

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

    public static bool HasInitializer(this IFieldSymbol @field)
    {
        var equalsSyntax = @field.DeclaringSyntaxReferences[0].GetSyntax() switch
        {
            PropertyDeclarationSyntax property => property.Initializer,
            VariableDeclaratorSyntax variable => variable.Initializer,
            _ => throw new Exception("Unknown declaration syntax")
        };

        if (
            equalsSyntax is null ||
            equalsSyntax.Value is null ||
            equalsSyntax.Kind() is not SyntaxKind.EqualsValueClause
        )
            return false;

        return true;
    }

    public static bool HasInitializer(this IPropertySymbol prop)
    {
        var equalsSyntax = prop.DeclaringSyntaxReferences[0].GetSyntax() switch
        {
            PropertyDeclarationSyntax property => property.Initializer,
            VariableDeclaratorSyntax variable => variable.Initializer,
            _ => throw new Exception("Unknown declaration syntax")
        };

        if (
            equalsSyntax is null ||
            equalsSyntax.Value is null ||
            equalsSyntax.Kind() is not SyntaxKind.EqualsValueClause
        )
            return false;

        return true;
    }
}