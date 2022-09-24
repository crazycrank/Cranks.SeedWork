namespace Cranks.SeedWork.Domain.Tests;

public class ValueObjectUnaryTest : UnitTestBase
{
    ////[Fact]
    ////public void CanInstantiateIntValueObject()
    ////{
    ////    var value = Faker.Random.Int();
    ////    var valueObject = new IntValueObject(value);

    ////    valueObject.ShouldNotBeNull();
    ////    valueObject.Value.ShouldBe(value);
    ////}

    ////[Fact]
    ////public void CanInstantiateStringValueObject()
    ////{
    ////    var value = Faker.Random.String();
    ////    var valueObject = new StringValueObject(value);

    ////    valueObject.ShouldNotBeNull();
    ////    valueObject.Value.ShouldBe(value);
    ////}

    ////[Fact]
    ////public void ValueObjectAreEqual()
    ////{
    ////    var value = Faker.Random.String();
    ////    var valueObject1 = new StringValueObject(value);
    ////    var valueObject2 = new StringValueObject(value);

    ////    valueObject1.ShouldBe(valueObject2);
    ////}

    ////[Fact]
    ////public void ValueObjectAreNotEqual()
    ////{
    ////    var valueObject1 = new StringValueObject(Faker.Random.String());
    ////    var valueObject2 = new StringValueObject(Faker.Random.String());

    ////    valueObject1.ShouldNotBe(valueObject2);
    ////}

    ////[Fact]
    ////public void CastToValueObject()
    ////{
    ////    var value = Faker.Random.Int();

    ////    var valueObject = (IntValueObject)value;

    ////    valueObject.Value.ShouldBe(value);
    ////}

    ////[Fact]
    ////public void CastFromValueObject()
    ////{
    ////    var valueObject = new IntValueObject(Faker.Random.Int());

    ////    var value = (int)valueObject;

    ////    value.ShouldBe(valueObject.Value);
    ////}

    private record Address(string Street, string PostCode, string City)
        : ValueObject<Address>
    {
    }
}
