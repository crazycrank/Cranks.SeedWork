using Bogus;

namespace Cranks.SeedWork.Domain.Tests;

public class UnitTestBase
{
    protected Faker Faker { get; } = new();
}
