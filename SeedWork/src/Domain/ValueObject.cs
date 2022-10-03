namespace Cranks.SeedWork.Domain;

public abstract record ValueObject;

public abstract record ValueObject<TValueObject>
    : ValueObject
    where TValueObject : ValueObject<TValueObject>;
