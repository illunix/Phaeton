using System.Diagnostics;
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

        sourceBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;\n");

        var classes = syntaxReceiver.GetSymbolsWithAttribute(
            ctx,
            nameof(GenerateInterfaceAndRegisterItAttribute)
        );
        if (!classes.Any())
            return;

        foreach (var @class in classes)
        {
            var namespaceName = @class.GetNamespaceName();
            if (string.IsNullOrEmpty(namespaceName))
                continue;

            if (namespaceName.Contains("Services;"))
                namespaceName = namespaceName.Replace("Services;", "Services.Abstractions");

            var attr = @class.GetAttributeByName(nameof(GenerateInterfaceAndRegisterItAttribute));
            if (attr is null)
                continue;

            var lifeTime = attr.GetAttributeConstructorArgValueByIndex(0);
            if (lifeTime is null)
                continue;

            registrationBuilder.AppendLine($"services.Add{((Lifetime)lifeTime).ToString()}<{$"{namespaceName}.{@class.BaseType.Name}"}, {@class}>();");
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
                continue;

            if (namespaceName.Contains("Services;"))
                namespaceName = namespaceName.Replace("Services;", "Services.Abstractions");

            var membersBuilder = new StringBuilder();

            foreach (var member in @class.GetMembersWithoutCtor())
            {
                switch (member.Kind)
                {
                    case SymbolKind.Property:
                        var prop = member as IPropertySymbol;

                        if (
                            !prop.CanBeReferencedByName &&
                            !prop.IsReadOnly
                        )
                            break;
                        else if (
                            prop.IsStatic ||
                            !prop.HasInitializer()
                        )
                            break;

                        membersBuilder.AppendLine($"\t\n\t\t{prop.Type} {member.Name} {{ get; set; }}");
                        break;
                    case SymbolKind.Field:
                        var field = member as IFieldSymbol;

                        if (field.Name.Contains("BackingField"))
                            break;

                        if (
                            !field.CanBeReferencedByName &&
                            !field.IsReadOnly
                        )
                            break;
                        else if (
                            field.IsStatic ||
                            !field.HasInitializer()
                        )
                            break;

                        if (!(@class.GetMemberConstructor() as IMethodSymbol).Parameters.Any(q => q.Name == field.Name))
                            break;

                        membersBuilder.AppendLine($"\t\n\t\t{field.Type} {member.Name} {{ get; set; }}");
                        break;
                    case SymbolKind.Method:
                        var method = member as IMethodSymbol;

                        if (method.IsStatic)
                            break;

                        if (
                           method.Name.StartsWith("get_") ||
                           method.Name.StartsWith("set_") ||
                           method.Name.StartsWith("private_set_") ||
                           method.Name.StartsWith("init_")
                        )
                            break;

                        var parameters = method.Parameters
                            .ToDictionary(
                                q => q.Name,
                                q => q.Type
                            );

                        membersBuilder.AppendLine($"\t\n\t\t{method.ReturnType} {member.Name}({(parameters.Any() ? string.Join(", ", parameters.Select(q => $"{q.Value} {q.Key}")) : string.Empty)});");
                        break;
                }
            }

            var attr = @class.GetAttributeByName(nameof(GenerateInterfaceAndRegisterItAttribute));
            if (attr is null)
                continue;

            var baseTypes = new List<string>();

            foreach (var value in attr.GetAttributeConstructorArgValuesByIndex(1))
                baseTypes.Add(value.ToString());

            interfacesBuilder.AppendLine(
                $"namespace {namespaceName}\n{{" +
                $"\n\tpublic interface {@class.BaseType.Name}{(baseTypes.Any() ? $" : {string.Join(", ", baseTypes)}" : string.Empty)}\n\t{{\t" +
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