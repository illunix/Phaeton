using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Text;
using System.Diagnostics;
using Phaeton.DependencyInjection.Generator;
using Phaeton.Generator.Extensions;

namespace Phaeton.DependencyInjection.Generator
{
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

            foreach (var @class in syntaxReceiver.GetSymbols(ctx))
            {
                if (@class
                    .GetMembers()
                    .OfType<IFieldSymbol>()
                    .Any(q =>
                        !q.CanBeReferencedByName &&
                        !q.IsReadOnly &&
                        q.IsStatic &&
                        q.HasInitializer()
                    )
                )
                    return;

                sourceBuilder.Append(CreateConstructor(@class));
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
            string namespaceName = @class.GetNamespaceName();

            var fields = @class.GetMembers().OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsReadOnly && !x.IsStatic && !x.HasInitializer())
                .Select(it => new { Namespace = it.ContainingNamespace.Name.Replace("Services", string.Empty), Type = it.Type.ToDisplayString(), ParameterName = ToCamelCase(it.Name), it.Name })
                .ToList();

            var arguments = fields.Select(it => $"{(it.Namespace.Contains("Service") ? $"{it.Namespace}.Abstractions.Services" : $"{it.Namespace}.Abstractions")}.{it.Type} {it.ParameterName}");

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
namespace {namespaceName}
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
}
