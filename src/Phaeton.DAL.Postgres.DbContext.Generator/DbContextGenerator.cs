using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Phaeton.DAL.Postgres.DbContext.Generator;

[Generator]
internal sealed class DbContextGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext ctx)
        => ctx.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext ctx)
    {
        if (ctx.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            return;

        var sb = new StringBuilder();

        var @class = syntaxReceiver.CandidateClasses
            .Select(classSyntax =>
                (INamedTypeSymbol)ctx.Compilation.GetSemanticModel(classSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classSyntax)!).TakeWhile(@class => @class is not null).Where(@class =>
                @class.GetAttributes().Any(q => q.AttributeClass?.Name == nameof(DbContextAttribute))).FirstOrDefault();

        var ogNamespace = @class.ContainingNamespace.ToDisplayString();
        var @namespace = ogNamespace.Replace("Context", "Abstractions.Context");

        sb.AppendLine(
@$"using Microsoft.EntityFrameworkCore;
using Phaeton.DAL.Postgres.Abstractions;

namespace Phaeton.DAL.Postgres
{{
    using {ogNamespace};
    using {@namespace};
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;

    public static class Extensions
    {{
        public static IServiceCollection AddPostgres(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {{
            services.AddPostgres<{$"I{@class.Name}"}, {@class.Name}>(configuration);
            return services;
        }}
    }}
}}
        "
        );

        var members = @class.GetMembers()
                        .OfType<IPropertySymbol>()
                        .Where(q =>
                            q.DeclaredAccessibility == Accessibility.Public &&
                            q.Type.Name == "DbSet"
                        )
                        .Select(q => $"public {q.Type} {q.Name} {{ get; init; }}")
        .ToList();

        sb.AppendLine(
@$"namespace {@namespace}
{{
    public interface I{@class.Name} : IDataContext
    {{
        {string.Join("\n", members)}          
    }}
}}

namespace {@class.ContainingNamespace}
{{
    using {@namespace};

    public partial class {@class.Name} : DbDataContext, I{@class.Name}
    {{
        public {@class.Name}(DbContextOptions<{@class.Name}> options) : base(options) {{ }}

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.ApplyConfigurationsFromAssembly(GetType().Assembly);      
    }}
}}
"
        );

        ctx.AddSource(
            "Phaeton.DAL.Postgres.DbContext.g.cs",
            SourceText.From(
                sb.ToString(),
                Encoding.UTF8
            )
        );
    }
}