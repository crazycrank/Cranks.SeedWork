namespace Cranks.SeedWork.Domain;

public abstract record ValueObject<TValueObject>
    where TValueObject : ValueObject<TValueObject>
{
}

public abstract record ValueObjectUnary<T, TValueObject>(T Value)
    where T : IEquatable<T>
    where TValueObject : ValueObjectUnary<T, TValueObject>
{
    public static implicit operator T(ValueObjectUnary<T, TValueObject> source)
    {
        return source.Value;
    }

    public static implicit operator ValueObjectUnary<T, TValueObject>?(T? source)
    {
        return Activator.CreateInstance(typeof(TValueObject), new[] { source }) as TValueObject;
    }
}
