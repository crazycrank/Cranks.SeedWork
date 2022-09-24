namespace Cranks.SeedWork.Domain;

public record ValueObject<T>(T Value)
    where T : IEquatable<T>;
