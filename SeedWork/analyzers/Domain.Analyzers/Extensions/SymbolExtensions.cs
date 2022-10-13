using Microsoft.CodeAnalysis;

namespace Cranks.SeedWork.Domain.Analyzers.Extensions;

internal static class SymbolExtensions
{
    public static bool IsMarkedAsValueObject(this ISymbol type)
    {
        return type.GetAttributes()
                   .Any(a => a is
                             {
                                 AttributeClass.Name: "ValueObjectAttribute",
                                 AttributeClass.ContainingAssembly.Name: "Cranks.SeedWork.Domain",
                                 AttributeClass.ContainingNamespace.Name: "Domain",
                             });
    }

    public static bool IsMarkedAsSmartEnum(this ISymbol type)
    {
        return type.GetAttributes()
                   .Any(a => a is
                             {
                                 AttributeClass.Name: "SmartEnumAttribute",
                                 AttributeClass.ContainingAssembly.Name: "Cranks.SeedWork.Domain",
                                 AttributeClass.ContainingNamespace.Name: "Domain",
                             });
    }

    public static bool IsValueObjectGenericBaseClass(this ISymbol type)
    {
        return type is INamedTypeSymbol
                       {
                           Name: "ValueObject",
                           ContainingAssembly.Name: "Cranks.SeedWork.Domain",
                           ContainingNamespace.Name: "Domain",
                           IsGenericType: true,
                       };
    }

    public static bool IsValueObjectBaseClass(this ISymbol type)
    {
        return type is INamedTypeSymbol
                       {
                           Name: "ValueObject",
                           ContainingAssembly.Name: "Cranks.SeedWork.Domain",
                           ContainingNamespace.Name: "Domain",
                           IsGenericType: false,
                       };
    }

    public static bool IsSmartEnumGenericBaseClass(this ISymbol type)
    {
        return type is INamedTypeSymbol
                       {
                           Name: "SmartEnum",
                           ContainingAssembly.Name: "Cranks.SeedWork.Domain",
                           ContainingNamespace.Name: "Domain",
                           IsGenericType: true,
                       };
    }

    public static bool IsSmartEnumBaseClass(this ISymbol type)
    {
        return type is INamedTypeSymbol
                       {
                           Name: "SmartEnum",
                           ContainingAssembly.Name: "Cranks.SeedWork.Domain",
                           ContainingNamespace.Name: "Domain",
                           IsGenericType: false,
                       };
    }

    public static string GetFullName(this ITypeSymbol type)
    {
        var typeName = type switch
                       {
                           INamedTypeSymbol { IsGenericType: true } ntp => $"{ntp.Name}<{string.Join("', ", ntp.TypeArguments.Select(p => p.Name))}>",
                           ITypeSymbol => type.Name,
                       };

        return $"{type.ContainingNamespace}.{typeName}";
    }
}
