using System.Collections.Immutable;
using System.Text;

using Cranks.SeedWork.Domain.Generator.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers;

[Generator(LanguageNames.CSharp)]
public class ValueObjectSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var valueObjects = context.SyntaxProvider
                                  .ForAttributeWithMetadataName($"{Namespaces.DomainAttributesNamespace}.{Classes.ValueObjectAttribute}",
                                                                (n, _) => n is RecordDeclarationSyntax rds && rds.IsPartial(),
                                                                GetValueObjectDetails)
                                  .Where(details => details is not null);

        var castOperatorTransforms = valueObjects.Where(details => details!.IsUnary)
                                                 .Select(CastOperatorsTransform!);

        var comparableTransforms = valueObjects.Where(details => details!.IsUnary && details.IsComparable)
                                               .Combine(context.CompilationProvider)
                                               .Select(ComparableTransform!);

        context.RegisterSourceOutput(castOperatorTransforms,
                                     (productionContext, transformed) =>
                                     {
                                         if (transformed is not (var details, string code))
                                         {
                                             return;
                                         }

                                         var prefix = details.IsGlobalNamespace ? string.Empty : $"{details.Namespace}.";
                                         productionContext.AddSource($"{prefix}{details.Name}.CastOperators.g.cs", code);
                                     });

        context.RegisterSourceOutput(comparableTransforms,
                                     (productionContext, transformed) =>
                                     {
                                         if (transformed is not (var details, string code))
                                         {
                                             return;
                                         }

                                         var prefix = details.IsGlobalNamespace ? string.Empty : $"{details.Namespace}.";
                                         productionContext.AddSource($"{prefix}{details.Name}.IComparable.g.cs", code);
                                     });
    }

    private static ValueObjectDetails? GetValueObjectDetails(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var parameters = new List<(string Name, ITypeSymbol Type)>();
        foreach (var parameter in (context.TargetNode as RecordDeclarationSyntax)?.ParameterList?.Parameters ?? Enumerable.Empty<ParameterSyntax>())
        {
            if (parameter.Type is null || context.SemanticModel.GetTypeInfo(parameter.Type, token).Type is not ITypeSymbol typeSymbol)
            {
                return null;
            }

            parameters.Add((parameter.Identifier.ValueText, typeSymbol));
        }

        var details = new ValueObjectDetails
                      {
                          Name = context.TargetSymbol.Name,
                          IsGlobalNamespace = context.TargetSymbol.ContainingNamespace.IsGlobalNamespace,
                          Namespace = context.TargetSymbol.ContainingNamespace.IsGlobalNamespace
                                          ? null
                                          : context.TargetSymbol.ContainingNamespace.Name,
                          Parameters = parameters.Select(p => (p.Name, p.Type.GetFullName())).ToImmutableList(),
                      };

        if (details.IsUnary)
        {
            details = details with { IsComparable = IsComparable(parameters.Single().Type) };
        }

        return details;

        static bool IsComparable(ITypeSymbol typeSymbol)
            => typeSymbol.Interfaces.Any(i => i is
                                              {
                                                  ContainingNamespace.Name: "System",
                                                  Name: "IComparable",
                                              });
    }

    private static (ValueObjectDetails Details, string Code)? CastOperatorsTransform(ValueObjectDetails details, CancellationToken token)
    {
        var code = new StringBuilder();
        code.AppendFileHeader()
            .AppendNamespace(details.IsGlobalNamespace, details.Namespace);

        using (new RecordContext(code, details.Name))
        {
            GenerateCastOperators(code, details);
        }

        return (details, code.ToString());
    }

    private static (ValueObjectDetails Details, string Code)? ComparableTransform((ValueObjectDetails Details, Compilation Compilation) input,
                                                                                  CancellationToken token)
    {
        var (details, compilation) = input;

        var code = new StringBuilder();
        code.AppendFileHeader()
            .AppendNamespace(details.IsGlobalNamespace, details.Namespace);

        var recordSymbol = compilation.GetTypeByMetadataName(details.FullName);
        if (recordSymbol is null)
        {
            return null;
        }

        var comparableOfSymbol = compilation.GetTypeByMetadataName("System.IComparable`1")?.Construct(recordSymbol);
        var comparableSymbol = compilation.GetTypeByMetadataName("System.IComparable");
        if (comparableSymbol is null || comparableOfSymbol is null)
        {
            return null;
        }

        using (new RecordContext(code, details.Name, comparableOfSymbol, comparableSymbol))
        {
            GenerateComparableMembers(code, details);
        }

        return (details, code.ToString());
    }

    private static void GenerateCastOperators(StringBuilder code,
                                              ValueObjectDetails details)
    {
        if (details.Parameters.Count != 1)
        {
            return;
        }

        code.AppendLine(
            @$"    public static explicit operator {details.Name}({details.Parameters.Single().Type} value)
    {{
        return new(value);
    }}

    public static explicit operator {details.Parameters.Single().Type}({details.Name} value)
    {{
        return value.{details.Parameters.Single().Name};
    }}");
    }

    private static void GenerateComparableMembers(StringBuilder code, ValueObjectDetails details)
    {
        var valueObjectName = details.Name;
        if (details.Parameters.Count != 1)
        {
            return;
        }

        code.AppendLine(
            @$"    public static bool operator <({valueObjectName}? left, {valueObjectName}? right)
    {{
        return System.Collections.Generic.Comparer<{valueObjectName}>.Default.Compare(left, right) < 0;
    }}

    public static bool operator >({valueObjectName}? left, {valueObjectName}? right)
    {{
        return System.Collections.Generic.Comparer<{valueObjectName}>.Default.Compare(left, right) > 0;
    }}

    public static bool operator <=({valueObjectName}? left, {valueObjectName}? right)
    {{
        return System.Collections.Generic.Comparer<{valueObjectName}>.Default.Compare(left, right) <= 0;
    }}

    public static bool operator >=({valueObjectName}? left, {valueObjectName}? right)
    {{
        return System.Collections.Generic.Comparer<{valueObjectName}>.Default.Compare(left, right) >= 0;
    }}

    public int CompareTo({valueObjectName}? other)
    {{
        if (ReferenceEquals(null, other))
        {{
            return 1;
        }}

        if (ReferenceEquals(this, other))
        {{
            return 0;
        }}

        return {details.Parameters.Single().Name}.CompareTo(other.{details.Parameters.Single().Type});
    }}

    public int CompareTo(object? obj)
    {{
        if (ReferenceEquals(null, obj))
        {{
            return 1;
        }}

        if (ReferenceEquals(this, obj))
        {{
            return 0;
        }}

        return obj is {valueObjectName} other ? CompareTo(other) : throw new System.ArgumentException($""Object must be of type {{nameof({valueObjectName})}}"");
    }}");
    }

    private record ValueObjectDetails
    {
        public string Name { get; init; } = null!;

        public bool IsGlobalNamespace { get; init; }

        public string? Namespace { get; init; }

        public string FullName => $"{(IsGlobalNamespace ? string.Empty : $"{Namespace}.")}{Name}";

        public ImmutableList<(string Name, string Type)> Parameters { get; init; } = ImmutableList<(string Name, string Type)>.Empty;

        public bool IsUnary => Parameters.Count == 1;

        public bool IsComparable { get; init; }
    }
}
