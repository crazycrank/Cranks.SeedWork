using Microsoft.CodeAnalysis;

namespace Cranks.SeedWork.Domain.Generator.Extensions;

internal static class SymbolExtensions
{
    public static bool IsValueObject(this ISymbol type)
        => type.GetAttributes()
               .Any(a => a.AttributeClass is
                         {
                             Name: "ValueObjectAttribute",
                             ContainingAssembly.Name: "Cranks.SeedWork.Domain",
                             ContainingNamespace.Name: "Attributes",
                         });
}
