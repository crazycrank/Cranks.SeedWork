using Microsoft.CodeAnalysis;

namespace Cranks.SeedWork.Domain.Generator.Extensions;

internal static class SymbolExtensions
{
    public static bool IsValueObject(this ISymbol type)
    {
        return type.GetAttributes()
                   .Any(a => a.AttributeClass is
                             {
                                 Name: Classes.ValueObjectAttribute,
                                 ContainingAssembly.Name: Assemblies.Domain,
                                 ContainingNamespace.Name: "Attributes",
                             });
    }
}
