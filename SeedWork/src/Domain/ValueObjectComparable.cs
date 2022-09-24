namespace Cranks.SeedWork.Domain;

public abstract record ValueObjectComparable<T>(T Value)
    : ValueObject<T>(Value)
    where T : IEquatable<T>, IComparable<T>;
