using Cranks.SeedWork.Domain.Attributes;

using Shouldly;

namespace Cranks.SeedWork.Domain.Test;

[SmartEnum]
public sealed partial record Gender(string Key) : SmartEnum<string>(Key)
{
    public static readonly Gender Unknown = new("unknown");
    public static readonly Gender Male = new("male");
    public static readonly Gender Female = new("female");
}

public class SmartEnumTests : UnitTestBase
{
    [Fact]
    public void SmartEnumGet_WhenFound_ReturnEnum()
    {
        Tests.Gender.Get(Gender.Male.Key).ShouldBe(Gender.Male);
    }

    [Fact]
    public void SmartEnumTryGet_WhenFound_ReturnTrue()
    {
        Tests.Gender.TryGet(Gender.Male.Key, out var result).ShouldBeTrue();
        result.ShouldBe(Gender.Male);
    }

    [Fact]
    public void SmartEnumGet_WhenNotFound_Throws()
    {
        Should.Throw<KeyNotFoundException>(() => Tests.Gender.Get(string.Empty));
    }

    [Fact]
    public void SmartEnum_WhenCastedToEnum_ReturnsEnum()
    {
        var result = (Gender?)Gender.Male.Key;
        result.ShouldBe(Gender.Male);
    }

    [Fact]
    public void SmartEnum_WhenCastedToEnum_AndNotExists_Throws()
    {
        Should.Throw<InvalidCastException>(() => (Gender)string.Empty);
    }

    [Fact]
    public void SmartEnumTryGet_WhenNotFound_ReturnsNull()
    {
        Tests.Gender.TryGet(string.Empty, out var result).ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void SmartEnumGetValues_ShouldContainAllValues()
    {
        Tests.Gender.AllValues.Count.ShouldBe(3);
        Tests.Gender.AllValues.ShouldContain(Gender.Unknown);
        Tests.Gender.AllValues.ShouldContain(Gender.Male);
        Tests.Gender.AllValues.ShouldContain(Gender.Female);
    }
}
