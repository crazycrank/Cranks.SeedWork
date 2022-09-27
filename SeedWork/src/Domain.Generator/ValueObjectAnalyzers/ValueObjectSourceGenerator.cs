﻿using System.Diagnostics;
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
        var transformedValueObjects = context.SyntaxProvider
                                             .ForAttributeWithMetadataName($"{Namespaces.DomainAttributesNamespace}.{Classes.ValueObjectAttribute}",
                                                                           (n, _) => n is RecordDeclarationSyntax rds && rds.IsPartial(),
                                                                           (c, _) => c)
                                             .Select(TransformRecord);

        context.RegisterSourceOutput(transformedValueObjects,
                                     (productionContext, tuple) =>
                                     {
                                         productionContext.AddSource(tuple.FileName, tuple.Code);
                                     });
    }

    private static (string FileName, string Code) TransformRecord(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var code = new StringBuilder();

        // auto-generated header
        code.AppendLine("// <auto-generated />");
        code.AppendLine("#nullable enable");
        code.AppendLine();

        // add namespace if necessary
        if (!context.TargetSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            code.AppendLine($"namespace {context.TargetSymbol.ContainingNamespace};");
            code.AppendLine();
        }

        // figure out if the user manually derived from ValueObject
        var alreadyDerivesFromValueObject = DerivesFromValueObject(context);
        var unaryDetails = GetUnaryDetails(context, token);
        var valueObjectName = context.TargetSymbol.Name;
        var isComparable = IsComparable(unaryDetails?.TypeSymbol);

        code.AppendLine($"partial record {valueObjectName}");

        var baseTypes = new List<string>();
        if (!alreadyDerivesFromValueObject)
        {
            baseTypes.Add($"Cranks.SeedWork.Domain.ValueObject<{valueObjectName}>");
        }

        if (isComparable)
        {
            baseTypes.Add($"System.IComparable<{valueObjectName}>");
            baseTypes.Add($"System.IComparable");
        }

        if (baseTypes.Any())
        {
            code.AppendLine($"    : {string.Join(", ", baseTypes)}");
        }

        code.AppendLine("{");
        if (unaryDetails is not null)
        {
            GenerateCastOperators(code, valueObjectName, unaryDetails.Value);
            if (isComparable)
            {
                GenerateComparableMembers(code, valueObjectName, unaryDetails.Value);
            }
        }

        code.AppendLine("}");

        return (GetFileName(context), code.ToString());
    }

    private static void GenerateCastOperators(StringBuilder code, string valueObjectName, (ITypeSymbol TypeSymbol, string UnaryName) unaryDetails)
    {
        code.AppendLine(@$"
    public static explicit operator {valueObjectName}({unaryDetails.TypeSymbol.GetFullName()} value)
    {{
        return new(value);
    }}

    public static explicit operator {unaryDetails.TypeSymbol.GetFullName()}({valueObjectName} value)
    {{
        return value.{unaryDetails.UnaryName};
    }}");
    }

    private static void GenerateComparableMembers(StringBuilder code, string valueObjectName, (ITypeSymbol TypeSymbol, string UnaryName) unaryDetails)
    {
        code.AppendLine(@$"
    public static bool operator <({valueObjectName}? left, {valueObjectName}? right)
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

        return {unaryDetails.UnaryName}.CompareTo(other.{unaryDetails.UnaryName});
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

    private static (ITypeSymbol TypeSymbol, string UnaryName)? GetUnaryDetails(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        // TODO can we do this from the target symbol?
        if (context.TargetNode is RecordDeclarationSyntax { ParameterList.Parameters: { Count: 1 } parameters })
        {
            var parameter = parameters.Single();
            var symbol = context.SemanticModel.GetSymbolInfo(parameter.Type!, cancellationToken).Symbol as ITypeSymbol;

            Debug.Assert(symbol is not null, "symbol is not null");

            return (symbol!, parameter.Identifier.Text);
        }

        return null;
    }

    private static bool DerivesFromValueObject(GeneratorAttributeSyntaxContext context)
    {
        return context.TargetSymbol is ITypeSymbol
                                       {
                                           BaseType:
                                           {
                                               Name: "ValueObject",
                                               ContainingAssembly.Name: "Cranks.SeedWork.Domain",
                                               ContainingNamespace.Name: "Domain",
                                           },
                                       };
    }

    private static bool IsComparable(ITypeSymbol? typeSymbol)
    {
        return typeSymbol?.Interfaces.Any(i => i is
                                               {
                                                   ContainingNamespace.Name: "System",
                                                   Name: "IComparable",
                                               })
               ?? false;
    }

    private static string GetFileName(GeneratorAttributeSyntaxContext context)
    {
        var ns = context.TargetSymbol.ContainingNamespace.IsGlobalNamespace
                     ? string.Empty
                     : $"{context.TargetSymbol.ContainingNamespace.Name}.";

        return $"{context.TargetSymbol.ContainingNamespace.ContainingAssembly.Name}.{ns}{context.TargetSymbol.MetadataName}.g.cs";
    }
}
