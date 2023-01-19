using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Phaeton.Generator.Extensions;

namespace Phaeton.DependencyInjection.Generator;

[Generator]
public sealed class InterfaceDefinitionsAndExtensionsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new InterfaceDefinitionsAndExtensionsSyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {
        if (ctx.SyntaxReceiver is not InterfaceDefinitionsAndExtensionsSyntaxReceiver syntaxReceiver)
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

            if (value.Contains("Services"))
                value = value.Replace(
                    "Services",
                    string.Empty
                );
        };

        foreach (var @class in classes)
        {
            var namespaceName = @class.GetNamespaceName();
            var hasServicesInNamespace = namespaceName.Contains("Services");

            if (string.IsNullOrEmpty(namespaceName))
                return;

            removeSemicolon(ref namespaceName);

            var attr = @class.GetAttributeByName(nameof(GenerateInterfaceAndRegisterItAttribute));
            if (attr is null)
                return;

            var serviceLifetime = attr.GetAttributeFirstConstructorArgValue();
            if (serviceLifetime is null)
                return;

            registrationBuilder.Append($"services.Add{((ServiceLifetime)serviceLifetime).ToString()}<{$"{namespaceName}{(hasServicesInNamespace ? "Abstractions.Services" : "Abstractions")}.{@class.BaseType.Name}"}, {@class}>();\n");
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
            var hasServicesInNamespace = namespaceName.Contains("Services");

            if (string.IsNullOrEmpty(namespaceName))
                return;

            removeSemicolon(ref namespaceName);

            var membersBuilder = new StringBuilder();

            foreach (var member in @class.GetMembersWithoutCtor())
            {
                switch (member.Kind)
                {
                    case SymbolKind.Property:
                        var prop = member as IPropertySymbol;

                        membersBuilder.AppendLine($"\t\n\t\t{prop.Type} {member.Name} {{ get; set; }}");
                        break;
                    case SymbolKind.Field:
                        var field = member as IFieldSymbol;

                        if (field.Name.Contains("BackingField"))
                            break;

                        if (
                            field.CanBeReferencedByName &&
                            field.IsReadOnly &&
                            !field.IsStatic &&
                            !field.HasInitializer()
                        )
                            break;

                        membersBuilder.AppendLine($"\t\n\t\t{field.Type} {member.Name} {{ get; set; }}");
                        break;
                    case SymbolKind.Method:
                        var method = member as IMethodSymbol;

                        if (
                           method.Name.StartsWith("get_") ||
                           method.Name.StartsWith("set_") ||
                           method.Name.StartsWith("private_set_") ||
                           method.Name.StartsWith("init_")
                        )
                            break;

                        membersBuilder.AppendLine($"\t\n\t\t{method.ReturnType} {member.Name}();");
                        break;
                }
            }

            interfacesBuilder.AppendLine(
                $"namespace {namespaceName}{(hasServicesInNamespace ? "Abstractions.Services" : "Abstractions.")}\n{{" +
                $"\n\tpublic interface {@class.BaseType.Name}\n\t{{\t" +
                $"{membersBuilder}\t}}\n}}\n"
            );
        }

        sourceBuilder.AppendLine(interfacesBuilder.ToString());

        ctx.AddSource(
            "Phaeton.Interfaces.g.cs",
            SourceText.From(
                sourceBuilder.ToString(),
                Encoding.UTF8
            )
        );
    }   
}       