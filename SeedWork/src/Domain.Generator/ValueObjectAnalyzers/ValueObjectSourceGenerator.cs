using System.Collections.Immutable;

using Cranks.SeedWork.Domain.Generator.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers;

[Generator]
public class ValueObjectSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO read https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
        var valueObjects = context.SyntaxProvider
                                  .CreateSyntaxProvider(CouldBeValueObject, GetSymbol)
                                  .Where(type => type is not null)
                                  .Collect();

        context.RegisterSourceOutput(valueObjects, GenerateCode);
    }

    private static bool CouldBeValueObject(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is not AttributeSyntax attribute)
        {
            return false;
        }

        if (attribute.Name.ExtractName() is not ("ValueObject" or "ValueObjectAttribute"))
        {
            return false;
        }

        if (attribute.GetAttributeNode() is not RecordDeclarationSyntax rds)
        {
            return false;
        }

        if (!rds.IsPartial())
        {
            return false;
        }

        if (!rds.IsSubTypeOf("ValueObject"))
        {
            return false;
        }

        return true;
    }

    private static ITypeSymbol? GetSymbol(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        if (attributeSyntax.GetAttributeNode() is not RecordDeclarationSyntax rds)
        {
            return null;
        }

        var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(rds, cancellationToken);
        return declaredSymbol is not ITypeSymbol symbol || !symbol.IsValueObject()
                   ? null
                   : symbol;
    }

    private static void GenerateCode(SourceProductionContext context,
                                     ImmutableArray<ITypeSymbol?> valueObjects)
    {
    }
}
