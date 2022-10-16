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

    public static bool DerivesFrom(this ITypeSymbol type, Predicate<INamedTypeSymbol> predicate, bool allowIndirectInheritance = true)
    {
        if (type.BaseType is null || type.BaseType.SpecialType is SpecialType.System_Object)
        {
            return false;
        }

        if (predicate(type.BaseType))
        {
            return true;
        }

        return allowIndirectInheritance && DerivesFrom(type.BaseType, predicate);
    }

    public static bool DerivesFrom(this ISymbol type, Predicate<INamedTypeSymbol> predicate, bool allowIndirectInheritance = true)
    {
        if (type is not INamedTypeSymbol typeSymbol)
        {
            return false;
        }

        return DerivesFrom(typeSymbol, predicate, allowIndirectInheritance);
    }

    public static bool DerivesFromValueObject(this ISymbol type)
    {
        return type.DerivesFrom(ts => ts is { Name: "ValueObject", ContainingAssembly.Name: "Cranks.SeedWork.Domain", ContainingNamespace.Name: "Domain" });
    }

    public static bool DerivesFromGenericSmartEnum(this ISymbol type)
    {
        return type.DerivesFrom(ts => ts is { Name: "SmartEnum", ContainingAssembly.Name: "Cranks.SeedWork.Domain", ContainingNamespace.Name: "Domain", IsGenericType: true },
                                false);
    }

    public static string GetFullName(this ITypeSymbol type)
    {
        var typeName = type switch
                       {
                           INamedTypeSymbol { IsGenericType: true } ntp => $"{ntp.Name}<{string.Join("', ", ntp.TypeArguments.Select(p => p.GetFullName()))}>",
                           ITypeSymbol => type.Name,
                       };

        var prependNamespace = type is not ITypeParameterSymbol;

        return prependNamespace ? $"{type.ContainingNamespace}.{typeName}" : typeName;
    }
}
