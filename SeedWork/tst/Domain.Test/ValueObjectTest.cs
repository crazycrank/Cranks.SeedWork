using Shouldly;

namespace Cranks.SeedWork.Domain.Tests;

[ValueObject]
public partial record Address(string Street, string ZipCode, string City) : ValueObject<Address>;

[ValueObject]
public partial record Age(int Value) : ValueObject<Age>;

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

    [Fact]
    public void GivenIntValueObject_CanBeCastToInt()
    {
        // arrange
        var age = new Age(Faker.Random.Int());

        // act
        var result = (int)age;

        // assert
        result.ShouldBe(age.Value);
    }

    [Fact]
    public void GivenInt_CanBeCastToIntValueObject()
    {
        // arrange
        var age = Faker.Random.Int();

        // act
        var result = (Age)age;

        // assert
        result.Value.ShouldBe(age);
    }

    [Fact]
    public void GivenValueObject_IsSubtypeOfValueObject()
    {
        // arrange
        var age = new Age(Faker.Random.Int());

        // assert
        age.ShouldBeAssignableTo<ValueObject<Age>>();
    }

    [Fact]
    public void GivenUnaryValueObjectWithComparableValue_ValueObjectShouldBeComparable()
    {
        var age1 = new Age(1);
        var age2 = new Age(2);

        age1.ShouldBeLessThan(age2);
        age1.ShouldBeLessThanOrEqualTo(age2);
        (age1 < age2).ShouldBeTrue();
        (age1 <= age2).ShouldBeTrue();
        (age1 > age2).ShouldBeFalse();
        (age1 >= age2).ShouldBeFalse();

        age2.ShouldBeGreaterThan(age1);
        age2.ShouldBeGreaterThanOrEqualTo(age1);
        (age2 < age1).ShouldBeFalse();
        (age2 <= age1).ShouldBeFalse();
        (age2 > age1).ShouldBeTrue();
        (age2 >= age1).ShouldBeTrue();
    }
}
