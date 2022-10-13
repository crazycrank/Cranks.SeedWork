using System.Collections.Immutable;
using System.Text;

using Cranks.SeedWork.Domain.Analyzers.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

namespace Cranks.SeedWork.Domain.Analyzers.Generators;

[Generator(LanguageNames.CSharp)]
public class SmartEnumSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if LAUNCH_DEBUGGER
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif

        var smartEnums = context.SyntaxProvider
                                .ForAttributeWithMetadataName(context,
                                                              "Cranks.SeedWork.Domain.SmartEnumAttribute",
                                                              (n, _) => n is RecordDeclarationSyntax rds && rds.IsPartial(),
                                                              GetSmartEnumDetails)
                                .Where(details => details is not null);

        var castOperatorTransforms = smartEnums.Select(CastOperatorsTransform!);
        var equalityTransforms = smartEnums.Select(EqualityTransform!);
        var valuesTransforms = smartEnums.Select(ValuesTransform!);

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

        context.RegisterSourceOutput(equalityTransforms,
                                     (productionContext, transformed) =>
                                     {
                                         if (transformed is not (var details, string code))
                                         {
                                             return;
                                         }

                                         var prefix = details.IsGlobalNamespace ? string.Empty : $"{details.Namespace}.";
                                         productionContext.AddSource($"{prefix}{details.Name}.Equality.g.cs", code);
                                     });

        context.RegisterSourceOutput(valuesTransforms,
                                     (productionContext, transformed) =>
                                     {
                                         if (transformed is not (var details, string code))
                                         {
                                             return;
                                         }

                                         var prefix = details.IsGlobalNamespace ? string.Empty : $"{details.Namespace}.";
                                         productionContext.AddSource($"{prefix}{details.Name}.Values.g.cs", code);
                                     });
    }

    private static SmartEnumDetails? GetSmartEnumDetails(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var enumInstances = (context.TargetSymbol as ITypeSymbol)?.GetMembers()
                                                                 .Where(member => member is IFieldSymbol
                                                                                            {
                                                                                                IsStatic: true,
                                                                                                DeclaredAccessibility: Accessibility.Public,
                                                                                                IsReadOnly: true,
                                                                                            })
                                                                 .Select(field => field.Name)
                                                                 .ToImmutableList();

        if (enumInstances is null)
        {
            return null;
        }

        if ((context.TargetSymbol as ITypeSymbol)?.BaseType is not INamedTypeSymbol baseType)
        {
            return null;
        }

        var details = new SmartEnumDetails
                      {
                          Name = context.TargetSymbol.Name,
                          IsGlobalNamespace = context.TargetSymbol.ContainingNamespace.IsGlobalNamespace,
                          Namespace = context.TargetSymbol.ContainingNamespace.ToString(),
                          EnumInstances = enumInstances,
                          KeyType = baseType.TypeArguments.Single().GetFullName(),
                      };

        return details;
    }

    private static (SmartEnumDetails Details, string Code)? CastOperatorsTransform(SmartEnumDetails details, CancellationToken token)
    {
        var code = new StringBuilder();
        code.AppendFileHeader()
            .AppendUsings()
            .AppendNamespace(details.IsGlobalNamespace, details.Namespace);

        using (new RecordContext(code, details.Name))
        {
            GenerateCastOperators();
        }

        return (details, code.ToString());

        void GenerateCastOperators()
            => code.AppendLine(
                @$"    public static explicit operator {details.Name}({details.KeyType} key)
    {{
        return {details.Name}.TryGet(key, out var @enum) ? @enum : throw new System.InvalidCastException($""Cannot cast '{{key}}' to {details.Name}"");
    }}

    public static explicit operator {details.KeyType}({details.Name} @enum)
    {{
        return @enum.Key;
    }}");
    }

    private static (SmartEnumDetails Details, string Code)? EqualityTransform(SmartEnumDetails details, CancellationToken token)
    {
        var code = new StringBuilder();
        code.AppendFileHeader()
            .AppendUsings()
            .AppendNamespace(details.IsGlobalNamespace, details.Namespace);

        using (new RecordContext(code, details.Name))
        {
            GenerateEqualityMembers();
        }

        return (details, code.ToString());

        void GenerateEqualityMembers()
            => code.AppendLine(
                @$"    public override int GetHashCode() => base.GetHashCode();

    public bool Equals({details.Name}? other) => base.Equals(other);");
    }

    private static (SmartEnumDetails Details, string Code)? ValuesTransform(SmartEnumDetails details, CancellationToken token)
    {
        var code = new StringBuilder();
        code.AppendFileHeader()
            .AppendUsings("System.Diagnostics.CodeAnalysis")
            .AppendNamespace(details.IsGlobalNamespace, details.Namespace);

        using (new RecordContext(code, details.Name))
        {
            GenerateValuesMembers();
        }

        return (details, code.ToString());

        void GenerateValuesMembers()
        {
            code.AppendLine(
                @$"    private static ImmutableList<{details.Name}>? _allValues;

    private static ImmutableList<{details.Name}> GetAllValues()
    {{
        var builder = ImmutableList.CreateBuilder<{details.Name}>();");

            foreach (var instanceName in details.EnumInstances)
            {
                code.AppendLine($"        builder.Add({instanceName});");
            }

            code.AppendLine(
                @$"        return builder.ToImmutable();
    }}

    public static ImmutableList<{details.Name}> AllValues => _allValues ??= GetAllValues();

    public static bool TryGet({details.KeyType} key, [NotNullWhen(true)] out {details.Name}? @enum)
    {{
        @enum = AllValues.SingleOrDefault(v => v.Key == key);
        return @enum is not null;
    }}

    public static {details.Name} Get({details.KeyType} key)
    {{
        if (TryGet(key, out var @enum))
        {{
            return @enum;
        }}

        throw new KeyNotFoundException($""No {{nameof({details.Name})}} with key {{key}} exists"");
    }}");
        }
    }

    private record SmartEnumDetails
    {
        public string Name { get; init; } = null!;

        public bool IsGlobalNamespace { get; init; }

        public string? Namespace { get; init; }

        public string KeyType { get; init; } = null!;

        public ImmutableList<string> EnumInstances { get; init; } = ImmutableList<string>.Empty;

        public string FullName => $"{(IsGlobalNamespace ? string.Empty : $"{Namespace}.")}{Name}";
    }
}
