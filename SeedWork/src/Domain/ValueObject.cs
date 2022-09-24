namespace Cranks.SeedWork.Domain;

public abstract record ValueObject<T>(T Value)
    where T : IEquatable<T>;
