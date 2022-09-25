namespace Cranks.SeedWork.Domain;

public abstract record ValueObject<TValueObject>
    where TValueObject : ValueObject<TValueObject>;

public abstract record ValueObjectUnary<T, TValueObject>(T Value)
    where T : IEquatable<T>
    where TValueObject : ValueObjectUnary<T, TValueObject>;
