namespace Cranks.SeedWork.Domain;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public abstract record SmartEnum;

public abstract record SmartEnum<TKey>(TKey Key)
    : SmartEnum
    where TKey : IEquatable<TKey>;
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
