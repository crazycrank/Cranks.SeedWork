using Cranks.SeedWork.Domain.Attributes;

using Shouldly;

namespace Cranks.SeedWork.Domain.Tests;

public class ValueObjectTest : UnitTestBase
{
    [Fact]
    public void GivenValueObject_WhenInstantiated_IsInstantiated()
    {
        // arrange
        var street = Faker.Address.StreetAddress();
        var zipCode = Faker.Address.ZipCode();
        var city = Faker.Address.City();

        // act
        var result = new Address(street, zipCode, city);

        // assert
        result.ShouldNotBeNull();
        result.Street.ShouldBe(street);
        result.ZipCode.ShouldBe(zipCode);
        result.City.ShouldBe(city);
    }

    [Fact]
    public void GivenTwoEqualValueObject_WhenCompared_AreEqual()
    {
        // arrange
        var street = Faker.Address.StreetAddress();
        var zipCode = Faker.Address.ZipCode();
        var city = Faker.Address.City();

        // act
        var result1 = new Address(street, zipCode, city);
        var result2 = new Address(street, zipCode, city);

        // assert
        result1.ShouldBe(result2);
    }

    [Fact]
    public void GivenTwoDifferentValueObject_WhenCompared_AreNotEqual()
    {
        // arrange

        // act
        var result1 = new Address(Faker.Address.StreetAddress(), Faker.Address.ZipCode(), Faker.Address.City());
        var result2 = new Address(Faker.Address.StreetAddress(), Faker.Address.ZipCode(), Faker.Address.City());

        // assert
        result1.ShouldNotBe(result2);
    }

    [ValueObject]
    private record Address(string Street, string ZipCode, string City)
    {
    }
}
