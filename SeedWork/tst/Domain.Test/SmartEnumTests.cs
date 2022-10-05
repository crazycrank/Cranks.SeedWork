namespace Cranks.SeedWork.Domain.Tests;

[SmartEnum]
public partial record Gender(string Key) : SmartEnum<string>(Key)
{
    public static readonly Gender Unknown = new("unknown");
    public static readonly Gender Male = new("male");
    public static readonly Gender Female = new("female");
}

public class SmartEnumTests : UnitTestBase
{
    [Fact]
    public void GivenSmartEnum()
    {
    }
}
