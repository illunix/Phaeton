using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Phaeton.Mediator.Generator;

[Generator]
internal sealed class MediatorSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {
        if (ctx.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            return;

        var sb = new StringBuilder();

        sb.AppendLine("using Phaeton.Abstractions;\n");

        var classes = syntaxReceiver.CandidateClasses
            .Select(classSyntax =>
                (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(classSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classSyntax)!).TakeWhile(@class => @class is not null).ToList();

        var classesWithAttr = syntaxReceiver.CandidateClasses
            .Select(classSyntax =>
                (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(classSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classSyntax)!).TakeWhile(@class => @class is not null).Where(@class =>
                @class.GetAttributes().Any(q => q.AttributeClass?.Name == nameof(GenerateMediatorAttribute))).ToList();
        if (!classesWithAttr.Any())
            return;

        foreach (var @class in classesWithAttr)
        {
            var @namespace = @class.ContainingNamespace.ToDisplayString();
            var handlerMethod = @class.GetMembers()
                .FirstOrDefault(q => q.Name == "Handler") as IMethodSymbol;

            if (handlerMethod?.ReturnType is not INamedTypeSymbol handlerMethodReturnType)
                return;

            var handlerMethodParams = handlerMethod.Parameters
                .ToDictionary(
                    q => q.Type,
                    q => q.Name
                );

            var handlerMethodParamsWithoutRequest = handlerMethodParams.Where(q =>
                q.Key.Name != "Command" &&
                q.Key.Name != "Query"
            ).ToList();

            var propertiesBuilder = new StringBuilder();

            foreach (var param in handlerMethodParamsWithoutRequest)
            {
                if (!param.Key
                    .ToDisplayString()
                    .StartsWith(@namespace.Split('.')[0])
                )
                {
                    var th = classes
                        .FirstOrDefault(q => q.BaseType.Name == param.Key.Name);
                    if (th is null)
                        continue;

                    propertiesBuilder.AppendLine($"private readonly {(th.ContainingNamespace.ToDisplayString() + ';').Replace("Services;", "Services.Abstractions")}.{param.Key} _{param.Value};");
                    continue;
                }

                propertiesBuilder.AppendLine($"private readonly {param.Key} _{param.Value};");
            }

            var requestBuilder = new StringBuilder();

            var commandExist = @class.GetMembers()
                .Any(q => q.Name == "Command");

            var queryExist = @class.GetMembers()
                .Any(q => q.Name == "Query");

            if (commandExist && queryExist)
                return;

            dynamic? type = null;

            if (handlerMethodReturnType.TypeArguments.Any())
                type = handlerMethodReturnType.TypeArguments.First();

            var @interface = "";
            if (commandExist)
                @interface = "ICommand";
            else if (queryExist)
                @interface = "IQuery";

            var requestInterface = type is not null
                ? $"{@interface}<{handlerMethodReturnType.TypeArguments.FirstOrDefault()}>"
                : $"{@interface}";

            if (@class.GetMembers().FirstOrDefault(q =>
                    q.Name == "Command" ||
                    q.Name == "Query") is not INamedTypeSymbol requestMethod
               )
                return;

            var requestMethodName = @class.GetMembers().Any(q => q.Name == "Command") ? "Command" : "Query";

            requestBuilder.Append($"public partial record {requestMethodName} : {requestInterface};");

            var validateMethodExist = requestMethod.GetMembers().Any(x => x.Name == "Validate");

            var constructorBuilder = new StringBuilder();

            var constructorParams = new List<string>();

            foreach (var param in handlerMethodParamsWithoutRequest)
            {
                if (!param.Key
                    .ToDisplayString()
                    .StartsWith(@namespace.Split('.')[0])
                )
                {
                    var th = classes
                        .FirstOrDefault(q => q.BaseType.Name == param.Key.Name);
                    if (th is null)
                        continue;

                    constructorParams.Add($"{(th.ContainingNamespace.ToDisplayString() + ';').Replace("Services;", "Services.Abstractions")}.{param.Key} {param.Value}");
                    continue;
                }

                constructorParams.Add($"{param.Key} {param.Value}");
            }

            var injected = string.Join("\n", handlerMethodParamsWithoutRequest.Select(q => $"_{q.Value} = {q.Value};"));

            var useConstructor = handlerMethodParamsWithoutRequest.Any();
            if (useConstructor)
                constructorBuilder.AppendLine(
                    $@"public {requestMethodName}HandlerCore({string.Join(", ", constructorParams)})
                    {{{injected}
                    }}"
                );

            var handlerBuilder = new StringBuilder();

            var handlerParams = string.Join(
                ", ",
                handlerMethodParams.Values.Select(q =>
                    q is "request" or "req" or "command" or "query" ? "request" : $"_{q}"
                )
            );

            var validateMethod = "";
            if (commandExist)
                validateMethod = "Command.Validate";
            else if (queryExist)
                validateMethod = "Query.Validate";

            var awaitHandlerMethod = $"await Handler({handlerParams});";

            handlerBuilder.Append(
@$"public async Task{(type is null ? string.Empty : $"<{type}>")} Handle({requestMethodName} request, CancellationToken cancellationToken) 
    {(type is null ?
        $"\t\t{(validateMethodExist ? $"{{\n\t\t\t\t{validateMethod}(request);\n\t\t\t\t{awaitHandlerMethod}\n\t\t\t}}" : $"\t\t\t=> {awaitHandlerMethod}")}" :
        $"\t\t\t=> {awaitHandlerMethod}")}"
            );

            var handlerInterface = "";
            if (commandExist)
                handlerInterface = "ICommandHandler";
            else if (queryExist)
                handlerInterface = "IQueryHandler";

            sb.AppendLine(
@$"namespace {@namespace}
{{
    public partial class {@class.Name} 
    {{
        {requestBuilder}

        private class {requestMethodName}HandlerCore : {handlerInterface}<{@class.Name}.{requestMethodName}{(type is null ? "" : $", {type}")}>
        {{
            {propertiesBuilder}{constructorBuilder}{handlerBuilder}
        }}
    }}
}}
"
            );
        }

        ctx.AddSource(
            "Phaeton.Mediator.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }
}