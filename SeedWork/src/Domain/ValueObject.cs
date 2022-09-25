namespace Cranks.SeedWork.Domain;

public abstract record ValueObject;

public abstract record ValueObject<TValueObject>
    : ValueObject
    where TValueObject : ValueObject<TValueObject>;

public abstract record ValueObjectUnary<T, TValueObject>(T Value)
    where T : IEquatable<T>
    where TValueObject : ValueObjectUnary<T, TValueObject>;
