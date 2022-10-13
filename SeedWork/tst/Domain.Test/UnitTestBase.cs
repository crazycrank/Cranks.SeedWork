using Bogus;

namespace Cranks.SeedWork.Domain.Test;

public class UnitTestBase
{
    protected Faker Faker { get; } = new();
}
