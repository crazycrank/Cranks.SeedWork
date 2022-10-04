﻿using Verify = Cranks.SeedWork.Domain.Generator.Tests.Verifiers.CSharpSourceGeneratorVerifier<
    Cranks.SeedWork.Domain.Generator.Generators.ValueObjectSourceGenerator>;

namespace Cranks.SeedWork.Domain.Generator.Tests.Generators;

public class ValueObjectGeneratorTest
{
    [Fact]
    public async Task UnaryValueObject_GeneratesCorrectCode()
    {
        var source = @"
using Cranks.SeedWork.Domain;
using Cranks.SeedWork.Domain;

namespace SomeNamespace;

[ValueObject]
public partial record TestValueObject(int Value) : ValueObject<TestValueObject>;
";

        var generatedCasts = @"// <auto-generated />
#nullable enable

namespace SomeNamespace;

partial record TestValueObject
{
    public static explicit operator TestValueObject(System.Int32 value)
    {
        return new(value);
    }

    public static explicit operator System.Int32(TestValueObject value)
    {
        return value.Value;
    }
}
";

        var generatedComparable = @"// <auto-generated />
#nullable enable

namespace SomeNamespace;

partial record TestValueObject
    : System.IComparable<TestValueObject>,
      System.IComparable
{
    public static bool operator <(TestValueObject? left, TestValueObject? right)
    {
        return System.Collections.Generic.Comparer<TestValueObject>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(TestValueObject? left, TestValueObject? right)
    {
        return System.Collections.Generic.Comparer<TestValueObject>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(TestValueObject? left, TestValueObject? right)
    {
        return System.Collections.Generic.Comparer<TestValueObject>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(TestValueObject? left, TestValueObject? right)
    {
        return System.Collections.Generic.Comparer<TestValueObject>.Default.Compare(left, right) >= 0;
    }

    public int CompareTo(TestValueObject? other)
    {
        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        return Value.CompareTo(other.Value);
    }

    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return 1;
        }

        if (ReferenceEquals(this, obj))
        {
            return 0;
        }

        return obj is TestValueObject other ? CompareTo(other) : throw new System.ArgumentException($""Object must be of type {nameof(TestValueObject)}"");
    }
}
";

        await Verify.VerifyGeneratorAsync(source,
                                          ("SomeNamespace.TestValueObject.CastOperators.g.cs", generatedCasts),
                                          ("SomeNamespace.TestValueObject.IComparable.g.cs", generatedComparable));
    }

    [Fact]
    public async Task NonUnary_GeneratesNoCode()
    {
        var source = @"
using Cranks.SeedWork.Domain;
using Cranks.SeedWork.Domain;


[ValueObject]
public partial record TestValueObject(int Value, string Another) : ValueObject<TestValueObject>;
";

        await Verify.VerifyGeneratorAsync(source);
    }
}