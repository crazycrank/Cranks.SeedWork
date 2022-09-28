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

    public static string GetFullName(this ITypeSymbol type)
    {
        var typeName = type switch
                       {
                           INamedTypeSymbol { IsGenericType: true } ntp => $"{ntp.Name}<{string.Join("', ", ntp.TypeArguments.Select(p => p.Name))}>",
                           ITypeSymbol => type.Name,
                       };

        return $"{type.ContainingNamespace.Name}.{typeName}";
    }
}
