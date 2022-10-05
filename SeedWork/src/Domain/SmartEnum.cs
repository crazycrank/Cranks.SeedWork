namespace Cranks.SeedWork.Domain;

public abstract record SmartEnum<TKey>(TKey Key)
    where TKey : IEquatable<TKey>;
