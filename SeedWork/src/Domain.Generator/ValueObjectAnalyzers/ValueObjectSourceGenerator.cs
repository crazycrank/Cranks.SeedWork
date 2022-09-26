﻿using System.Text;

using Cranks.SeedWork.Domain.Generator.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers;

[Generator]
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
        code.AppendLine();

        // add namespace if necessary
        if (!context.TargetSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            code.AppendLine($"namespace {context.TargetSymbol.ContainingNamespace};");
            code.AppendLine();
        }

        // figure out if the user manually derived from ValueObject
        var alreadyDerivesFromValueObject = DerivesFromValueObject(context);
        var unaryDetails = GetUnaryDetails(context);
        var valueObjectName = context.TargetSymbol.Name;

        code.AppendLine($"partial record {valueObjectName}");

        if (!alreadyDerivesFromValueObject)
        {
            code.AppendLine($"    : Cranks.SeedWork.Domain.ValueObject<{valueObjectName}>");
        }

        ////TODO: Additional base interfaces for unary value objects (IComparable...)

        code.AppendLine("{");
        if (unaryDetails is not null)
        {
            code.AppendLine("    // TODO implement unary extensions");
            GenerateCastOperators(code, valueObjectName, unaryDetails.Value);
        }

        code.AppendLine("}");

        return (GetFileName(context), code.ToString());
    }

    private static void GenerateCastOperators(StringBuilder code, string valueObjectName, (TypeSyntax UnaryType, string UnaryName) unaryDetails)
    {
        code.AppendLine(@$"
    public static implicit operator {valueObjectName}({unaryDetails.UnaryType} value)
    {{
        return new(value);
    }}
    public static implicit operator {unaryDetails.UnaryType}({valueObjectName} value)
    {{
        return value.{unaryDetails.UnaryName};
    }}");
    }

    private static (TypeSyntax UnaryType, string UnaryName)? GetUnaryDetails(GeneratorAttributeSyntaxContext context)
    {
        // TODO can we do this from the target symbol?
        if (context.TargetNode is RecordDeclarationSyntax { ParameterList.Parameters: { Count: 1 } parameters })
        {
            var parameter = parameters.Single();
            return (parameter.Type!, parameter.Identifier.Text);
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
                                               ContainingNamespace.Name: "Cranks.SeedWork.Domain",
                                           },
                                       };
    }

    private static string GetFileName(GeneratorAttributeSyntaxContext context)
    {
        var ns = context.TargetSymbol.ContainingNamespace.IsGlobalNamespace
                     ? string.Empty
                     : $"{context.TargetSymbol.ContainingNamespace.Name}.";

        return $"{ns}{context.TargetSymbol.MetadataName}.g.cs";
    }
}
