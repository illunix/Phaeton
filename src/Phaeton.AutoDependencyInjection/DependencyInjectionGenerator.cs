using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Phaeton.AutoDependencyInjection.Generator;
using Phaeton.Generator.Extensions;

namespace Phaeton.DependencyInjection.Generator;

[Generator]
public sealed class DependencyInjectionGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif 
        if (ctx.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            return;

        var sourceBuilder = new StringBuilder();
        var registrationBuilder = new StringBuilder();

        sourceBuilder.AppendLine("using Phaeton.DependencyInjection;\n");

        var classes = syntaxReceiver.GetSymbolsWithAttribute(
            ctx,
            nameof(GenerateInterfaceAndRegisterItAttribute)
        );

        var removeSemicolon = (ref string value) =>
        {
            value = value.Replace(
                ";",
                string.Empty
            );
        };

        foreach (var @class in classes)
        {
            var namespaceName = @class.GetNamespaceName();
            if (string.IsNullOrEmpty(namespaceName))
                return;

            removeSemicolon(ref namespaceName);

            var attr = @class.GetAttributeByName(nameof(GenerateInterfaceAndRegisterItAttribute));
            if (attr is null)
                return;

            var serviceLifetime = attr.GetAttributeFirstConstructorArgValue();
            if (serviceLifetime is null)
                return;

            registrationBuilder.AppendLine("services");
            registrationBuilder.AppendLine($"\t\t\t\t.Add{((ServiceLifetime)serviceLifetime).ToString()}<{@class}, {$"{namespaceName}.Abstractions.{@class.BaseType.Name}"}>();\n");
        }

        registrationBuilder.AppendLine("\t\t\treturn services;");

        sourceBuilder.AppendLine(
            "namespace Phaeton.DependencyInjection\n" + 
            "{\n\tpublic static class Extensions\n" +
            "\t{\n\t\tpublic static IServiceCollection RegisterServicesFromAssembly(this IServiceCollection services)\n" + 
            $"\t\t{{\n\t\t\t{registrationBuilder.ToString()}\t\t}}\n\t}}\n}}\n" 
        );

        var interfacesBuilder = new StringBuilder();

        foreach (var @class in classes)
        {
            var namespaceName = @class.GetNamespaceName();
            if (string.IsNullOrEmpty(namespaceName))
                return;

            removeSemicolon(ref namespaceName);

            var membersBuilder = new StringBuilder();

            foreach (var member in @class.GetMembers())
            {
                switch (member.Kind)
                {
                    case SymbolKind.Method:
                        member = member;
                        break;
                }
                if (handlerMethod?.ReturnType is not INamedTypeSymbol handlerMethodReturnType)
                {
                    return string.Empty;
                }

                membersBuilder.AppendLine($"\t{member} {member.Name}();");
            }

            interfacesBuilder.AppendLine(
                $"namespace {namespaceName}.Abstractions\n{{" +
                $"\n\tpublic interface {@class.BaseType.Name}\n\t{{\n\t" +
                $"{membersBuilder}}}\n}}"
            );
        }

        sourceBuilder.AppendLine(interfacesBuilder.ToString());

        ctx.AddSource(
            "Phaeton.DependencyInjection.g.cs",
            SourceText.From(
                sourceBuilder.ToString(),
                Encoding.UTF8
            )
        );
    }   
}       