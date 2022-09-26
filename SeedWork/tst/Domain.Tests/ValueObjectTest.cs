using Cranks.SeedWork.Domain.Attributes;

using Shouldly;

namespace Cranks.SeedWork.Domain.Tests;

[ValueObject]
public partial record Address(string Street, string ZipCode, string City) : ValueObject<Address>;

[ValueObject]
public partial record Age(int Value);

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
    public void GivenIntValueObject_CanBeCastedToInt()
    {
        // arrange
        var age = new Age(Faker.Random.Int());

        // act
        var result = (int)age;

        // assert
        result.ShouldBe(age.Value);
    }

    [Fact]
    public void GivenInt_CanBeCastedToIntValueObject()
    {
        // arrange
        var age = Faker.Random.Int();

        // act
        var result = (Age)age;

        // assert
        result.Value.ShouldBe(age);
    }

    ////[ValueObject]
    ////private partial record Age(int Value) : ValueObject<Age>,
    ////                                        IComparable<Age>,
    ////                                        IComparable
    ////{
    ////    // if unitary value object
    ////    public static implicit operator Age(int age)
    ////    {
    ////        return new Age(age);
    ////    }

    ////    public static implicit operator int(Age age)
    ////    {
    ////        return age.Value;
    ////    }

    ////    // if value is comparable
    ////    public static bool operator <(Age? left, Age? right)
    ////    {
    ////        return Comparer<Age>.Default.Compare(left, right) < 0;
    ////    }

    ////    public static bool operator >(Age? left, Age? right)
    ////    {
    ////        return Comparer<Age>.Default.Compare(left, right) > 0;
    ////    }

    ////    public static bool operator <=(Age? left, Age? right)
    ////    {
    ////        return Comparer<Age>.Default.Compare(left, right) <= 0;
    ////    }

    ////    public static bool operator >=(Age? left, Age? right)
    ////    {
    ////        return Comparer<Age>.Default.Compare(left, right) >= 0;
    ////    }

    ////    public int CompareTo(Age? other)
    ////    {
    ////        if (ReferenceEquals(null, other))
    ////        {
    ////            return 1;
    ////        }

    ////        if (ReferenceEquals(this, other))
    ////        {
    ////            return 0;
    ////        }

    ////        return Value.CompareTo(other.Value);
    ////    }

    ////    public int CompareTo(object? obj)
    ////    {
    ////        if (ReferenceEquals(null, obj))
    ////        {
    ////            return 1;
    ////        }

    ////        if (ReferenceEquals(this, obj))
    ////        {
    ////            return 0;
    ////        }

    ////        return obj is Age other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Age)}");
    ////    }

    ////    // if value supports Parse
    ////    public static bool TryParse([NotNullWhen(true)] string? s, out Age result)
    ////    {
    ////        var parsed = int.TryParse(s, out var intResult);
    ////        result = new Age(intResult);
    ////        return parsed;
    ////    }
    ////}
}
