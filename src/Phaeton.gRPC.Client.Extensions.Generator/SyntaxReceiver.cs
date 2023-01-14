using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Phaeton.gRPC.Server.Extensions.Generator;

internal sealed class SyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax { Members.Count: > 0 } classDeclarationSyntax)
            CandidateClasses.Add(classDeclarationSyntax);
    }
}