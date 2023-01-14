using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Phaeton.gRPC.Server.Extensions.Generator;

namespace Phaeton.GenerateMediator.Generator;

[Generator]
internal sealed class MediatorSourceGenerator : ISourceGenerator
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

        var sb = new StringBuilder();
        
        var classes = syntaxReceiver.CandidateClasses
            .Select(classSyntax =>
                (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(classSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classSyntax)!).TakeWhile(@class => @class is not null).Where(@class =>
                @class.GetAttributes().Any(q => q.AttributeClass?.Name == nameof(GenerateMediatorAttribute))).ToList();

        foreach (var @class in classes)
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

            foreach (var param in handlerMethodParams.Where(q =>
                         q.Key.Name != "Command" && 
                         q.Key.Name != "Query"
            ))
                propertiesBuilder.AppendLine($"private readonly {param.Key} _{param.Value};");
            
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
            
            var constructorBuilder = new StringBuilder();

            var constructorParams = string.Join(", ", handlerMethodParamsWithoutRequest.Select(q => $"{q.Key} {q.Value}"));
            var injected = string.Join("\n", handlerMethodParamsWithoutRequest.Select(q => $"_{q.Value} = {q.Value};"));

            var useConstructor = handlerMethodParamsWithoutRequest.Any();
            if (useConstructor)
            {
                constructorBuilder.AppendLine(
                    $@"public {requestMethodName}HandlerCore({constructorParams})
                    {{{injected}
                    }}"
                ); 
            }
            
            var handleBuilder = new StringBuilder();

            var handlerParams =  string.Join(
                ", ", 
                    handlerMethodParams.Values.Select(q => 
                        q is "request" or "req" or "command" or "query" ? "request" : $"_{q}"
                    )
                );

            handleBuilder.Append(
                @$"public async Task<{type ?? ""}> Handle({requestMethodName} request, CancellationToken cancellationToken) 
            {{
                {(type is null ? $"await Handler({handlerParams});\n\n\t\t\t\treturn Unit.Value;" :
                    $"return await Handler({handlerParams});")}
            }}"
            );

            var handlerInterface = "";
            if (commandExist)
                handlerInterface = "ICommandHandler";
            else if (queryExist)
                handlerInterface = "IQueryHandler";
            
            sb.AppendLine(
 @$"
using Phaeton.Abstractions;

namespace {@namespace}
{{
    public partial class {@class.Name} 
    {{
        {requestBuilder}
        private class {requestMethodName}HandlerCore : {handlerInterface}<{@class.Name}.{requestMethodName}{(type is null ? "" : $", {type}")}>
        {{
            {propertiesBuilder}{constructorBuilder}
            {handleBuilder}
        }}
    }}
}}
"
            );
        }

        ctx.AddSource(
            "Phaeton.Mediator.Generator.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }
}