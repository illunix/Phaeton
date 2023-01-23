using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Text;
using Phaeton.Generator.Extensions;

namespace Phaeton.DependencyInjection.Generator;

[Generator]
internal sealed class ConstructorInjectionGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForSyntaxNotifications(() => new ConstructorInjectionGeneratorSyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {
        if (ctx.SyntaxReceiver is not ConstructorInjectionGeneratorSyntaxReceiver syntaxReceiver)
            return;

        var sourceBuilder = new StringBuilder();

        var classes = syntaxReceiver.GetSymbols(ctx);
        if (!classes.Any())
            return;

        foreach (var @class in classes)
        {
            var fields = @class
                .GetMembers()
                .OfType<IFieldSymbol>()
                .ToList();
            foreach (var field in fields)
            {
                if (
                    !field.CanBeReferencedByName &&
                    !field.IsReadOnly &&
                    field.DeclaredAccessibility is not Accessibility.Private 
                )
                    continue;
                else if (
                    field.IsStatic ||
                    field.HasInitializer()
                )
                    continue;

                sourceBuilder.AppendLine(CreateConstructor(@class));
            }
        }

        ctx.AddSource(
            "Phaeton.ConstructorInjections.g.cs",
            SourceText.From(
                sourceBuilder.ToString(),
                Encoding.UTF8)
        );
    }

    private string CreateConstructor(INamedTypeSymbol @class)
    {
        string ogNamespaceName = @class.GetNamespaceName();
        string classNamespaceName = ogNamespaceName.Replace(";", string.Empty);
        string interfaceNamespaceName = ogNamespaceName;
        if (interfaceNamespaceName.Contains("Services;"))
            interfaceNamespaceName = interfaceNamespaceName.Replace("Services;", "Abstractions.Services");

        var fields = @class.GetMembers().OfType<IFieldSymbol>()
            .Where(x => x.CanBeReferencedByName && x.IsReadOnly && !x.IsStatic && !x.HasInitializer())
            .Select(it => new { Namespace = interfaceNamespaceName, Type = it.Type.ToDisplayString(), ParameterName = ToCamelCase(it.Name), it.Name })
            .ToList();

        var arguments = new List<string>();

        foreach (var field in fields)
        {
            var @namespace = "";

            var isNamespaceStartsWithSameChars = field.Namespace.StartsWith(field.Type.Split('.')[0]);

            if (
                !isNamespaceStartsWithSameChars &&
                field.Type.Contains('.')
            )
                @namespace = field.Type;
            else
                @namespace = $"{field.Namespace}.{field.Type}";

            if (
                field.Namespace.Contains("Services;") &&
                isNamespaceStartsWithSameChars
            )
                @namespace = field.Namespace.Replace(
                    "Services;", 
                    "Abstractions.Services"
                );
            else if (isNamespaceStartsWithSameChars)
                @namespace = field.Namespace;

            arguments.Add($"{@namespace} {field.ParameterName}");
        }

        // var arguments = fields.Select(it => $"{(it.Namespace.Contains("Services;") ? it.Namespace.Replace("Services;", "Abstractions.Services") : it.Namespace)}.{it.Type} {it.ParameterName}");

        var injections = new StringBuilder();

        foreach (var field in fields)
            injections.Append($@"this.{field.Name} = {field.ParameterName};");

        var symbolTypeArgs = new StringBuilder();

        if (@class.TypeArguments.Any())
        {
            foreach (var arg in @class.TypeArguments)
            {
                if (@class.TypeArguments.Count() == 1)
                {
                    symbolTypeArgs.Append($"<{arg.Name}>");
                }
                else
                {
                    if (arg == @class.TypeArguments.Last())
                    {
                        symbolTypeArgs.Append($"{arg.Name}>");
                    }
                    else
                    {
                        symbolTypeArgs.Append($"<{arg.Name}, ");
                    }
                }
            }
        }

        var source = new StringBuilder($@"
namespace {classNamespaceName}
{{
    public partial class {@class.Name}{symbolTypeArgs}
    {{
        public {@class.Name}({string.Join(", ", arguments)})
        {{
            {injections}
        }}
    }}
}}
");

        return source.ToString();
    }

    private static string ToCamelCase(string name)
    {
        name = name.TrimStart('_');
        return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
    }
}