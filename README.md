# Cranks.SeedWork

This library is intendended to provide a usefull set of base classes and support to write your own applications based on the Domain Driven Design principle.
It is intended to be primarily used in new applications, so it targets .NET 6.0 right now, and future version will most probably bump that even further, so it can make use of modern language features.

It uses source generators to simplify a lot of the boilerplate you would have to normally write. 

# Packages
## [Cranks.SeedWork.Domain](https://www.nuget.org/packages/Cranks.SeedWork.Domain/)
The only available package and still in a very early alpha phase. It provides support to write value objects and smart enums

# Usage
## Domain
### ValueObjects
Define a value object like this
```csharp
[ValueObject] // Without this, no source generation or analyzers are used.
public partial record Money(decimal Value) : ValueObject<Money>;
```
When there is only a single parameter in the primary constructor, the provided source generators automatically generate cast operators from and to the type of parameter type.
If the type is IComparable, it also generates automatic forwards to the IComparable implementation of the parameter type as well as comparison operators.

Value objects can also have multiple values
```csharp
[ValueObject] // Without this, no source generation or analyzers are used.
public partial record Address(string street, string zipcode, string city) : ValueObject<Address>;
```

For value objects with more than one parameter, there is currently no automatic code generation.

### SmartEnums
Smart enums are extended enumeration classes which provide more functionality then the base `enum` types of C#. They can be defined like this.
```csharp
[SmartEnum]
public sealed partial record AddressType(string Key)
    : SmartEnum<string>(Key) // the base class needs to know the key type of the enumeration
{
    public static readonly AddressType Unknown = new("unknown"); // Enum instances need to be public static readonly
    public static readonly AddressType Private = new("private"); // it's recommended not to use nameof here, since it will limit you from renaming the enums
    public static readonly AddressType Business = new("business");
}
```

you can use it like
```csharp
var private = AddressType.Private;

var allAdressTypes = AddressType.AllValues; // AllValues returns all enum instances

// TryGet and Get work similar to Parse/TryParse
var tryget = AddressType.TryGet("business", out var found) ? found : null;
var get = AddressType.Get("invalid value"), // throws

```