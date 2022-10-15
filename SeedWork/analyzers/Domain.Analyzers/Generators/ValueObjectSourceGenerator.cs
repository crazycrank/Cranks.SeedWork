using System.Collections.Immutable;
using System.Text;
using Cranks.SeedWork.Domain.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

namespace Cranks.SeedWork.Domain.Analyzers.Generators;

[Generator(LanguageNames.CSharp)]
public class ValueObjectSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if LAUNCH_DEBUGGER
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif

        var valueObjects = context.SyntaxProvider
                                  .ForAttributeWithMetadataName(context,
                                                                "Cranks.SeedWork.Domain.ValueObjectAttribute",
                                                                (n, _) => n is RecordDeclarationSyntax rds && rds.IsPartial(),
                                                                GetValueObjectDetails)
                                  .Where(details => details is not null);

        var castOperatorTransforms = valueObjects.Where(details => details!.IsUnary)
                                                 .Select(CastOperatorsTransform!);

        var comparableTransforms = valueObjects.Where(details => details!.IsUnary && details.Comparable is not (false, false))
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
        if (context.TargetNode is not RecordDeclarationSyntax rds)
        {
            return null;
        }

        var parameters = new List<(string Name, ITypeSymbol Type)>();
        foreach (var parameter in rds.ParameterList?.Parameters ?? Enumerable.Empty<ParameterSyntax>())
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
                          MetadataName = context.TargetSymbol.MetadataName,
                          IsAbstract = context.TargetSymbol.IsAbstract,
                          IsGlobalNamespace = context.TargetSymbol.ContainingNamespace.IsGlobalNamespace,
                          Namespace = context.TargetSymbol.ContainingNamespace.ToString(),
                          Parameters = parameters.Select(p => (p.Name, p.Type.GetFullName())).ToImmutableList(),
                          TypeParameterList = rds.TypeParameterList?.ToString() ?? string.Empty,
                      };

        if (details.IsUnary)
        {
            details = details with { Comparable = IsComparable(parameters.Single().Type) };
        }

        return details;

        static (bool IsComparable, bool IsComparableOfT) IsComparable(ITypeSymbol typeSymbol)
        {
            // we need no check both the normale interface declarations
            var interfaces = typeSymbol.AllInterfaces;

            // and - if the symbol is a TypeParameter - also also need to check all interfaces of type constraints
            if (typeSymbol is ITypeParameterSymbol tps)
            {
                interfaces = interfaces.Concat(
                                           tps.ConstraintTypes.Concat(tps.ConstraintTypes.SelectMany(t => t.AllInterfaces))
                                              .Cast<INamedTypeSymbol>()
                                              .Where(s => s is not null))
                                       .ToImmutableArray();
            }

            bool isComparable = false, isComparableOfT = false;

            if (interfaces.Any(i => i is { ContainingNamespace.Name: "System", Name: "IComparable", IsGenericType: false }))
            {
                isComparable = true;
            }

            if (interfaces.Any(i => i is { ContainingNamespace.Name: "System", Name: "IComparable", IsGenericType: true }))
            {
                isComparableOfT = true;
            }

            return (isComparable, isComparableOfT);
        }
    }

    private static (ValueObjectDetails Details, string Code)? CastOperatorsTransform(ValueObjectDetails details, CancellationToken token)
    {
        var code = new StringBuilder();
        code.AppendFileHeader()
            .AppendUsings()
            .AppendNamespace(details.IsGlobalNamespace, details.Namespace);

        using (new RecordContext(code, details.Name, details.TypeParameterList))
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
            .AppendUsings()
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

        using (new RecordContext(code,
                                 details.Name,
                                 details.TypeParameterList,
                                 comparableOfSymbol,
                                 comparableSymbol))
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

        if (!details.IsAbstract)
        {
            code.AppendLine(
                @$"    public static explicit operator {details.Name}{details.TypeParameterList}({details.Parameters.Single().Type} value)
    {{
        return new(value);
    }}");
        }

        code.AppendLine(
            @$"    public static implicit operator {details.Parameters.Single().Type}({details.Name}{details.TypeParameterList} value)
    {{
        return value.{details.Parameters.Single().Name};
    }}");
    }

    private static void GenerateComparableMembers(StringBuilder code, ValueObjectDetails details)
    {
        var valueObjectName = $"{details.Name}{details.TypeParameterList}";
        if (details.Parameters.Count != 1)
        {
            return;
        }

        if (!details.IsGenericTypeDefinition)
        {
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
    }}");
        }

        if (details.Comparable.IsComparableOfT)
        {
            code.AppendLine(
                @$"    public int CompareTo({valueObjectName}? other)
    {{
        if (ReferenceEquals(null, other))
        {{
            return 1;
        }}

        if (ReferenceEquals(this, other))
        {{
            return 0;
        }}

        return {details.Parameters.Single().Name}.CompareTo(other.{details.Parameters.Single().Name});
    }}");
            code.AppendLine(
                @$"    public int CompareTo(object? obj)
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
    }

    private record ValueObjectDetails
    {
        public string Name { get; init; } = null!;

        public string MetadataName { get; init; } = null!;

        public bool IsGlobalNamespace { get; init; }

        public string? Namespace { get; init; }

        public string FullName => $"{(IsGlobalNamespace ? string.Empty : $"{Namespace}.")}{MetadataName}";

        public ImmutableList<(string Name, string Type)> Parameters { get; init; } = ImmutableList<(string Name, string Type)>.Empty;

        public bool IsUnary => Parameters.Count == 1;

        public (bool IsComparable, bool IsComparableOfT) Comparable { get; init; }

        public string TypeParameterList { get; init; } = string.Empty;

        public bool IsGenericTypeDefinition => !string.IsNullOrEmpty(TypeParameterList);

        public bool IsAbstract { get; init; }
    }
}
