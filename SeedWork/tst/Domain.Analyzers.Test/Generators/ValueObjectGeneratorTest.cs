﻿using Verify = Cranks.SeedWork.Domain.Analyzers.Test.Verifiers.CSharpSourceGeneratorVerifier<
    Cranks.SeedWork.Domain.Analyzers.Generators.ValueObjectSourceGenerator>;

namespace Cranks.SeedWork.Domain.Analyzers.Test.Generators;

public class ValueObjectGeneratorTest
{
    [Fact]
    public async Task UnaryValueObject_GeneratesCorrectCode()
    {
        var source = @"
using Cranks.SeedWork.Domain;

namespace SomeNamespace;

[ValueObject]
public partial record TestValueObject(int Value) : ValueObject;
";

        var generatedCasts = @"// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SomeNamespace;

partial record TestValueObject
{
    public static explicit operator TestValueObject(System.Int32 value)
    {
        return new(value);
    }
    public static implicit operator System.Int32(TestValueObject value)
    {
        return value.Value;
    }
}
";

        var generatedComparable = @"// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SomeNamespace;

partial record TestValueObject
    : System.IComparable<SomeNamespace.TestValueObject>,
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

[ValueObject]
public partial record TestValueObject(int Value, string Another) : ValueObject;
";

        await Verify.VerifyGeneratorAsync(source);
    }

    [Fact]
    public async Task GenericValueObject_GeneratesCorrectCode()
    {
        var source = @"
using Cranks.SeedWork.Domain;
using System;

namespace SomeNamespace;

[ValueObject]
public partial record TestValueObject<TValue>(TValue Value) : ValueObject
    where TValue : IComparable<TValue>;
";

        var generatedCasts = @"// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SomeNamespace;

partial record TestValueObject<TValue>
{
    public static explicit operator TestValueObject<TValue>(TValue value)
    {
        return new(value);
    }
    public static implicit operator TValue(TestValueObject<TValue> value)
    {
        return value.Value;
    }
}
";

        var generatedComparable = @"// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SomeNamespace;

partial record TestValueObject<TValue>
    : System.IComparable<SomeNamespace.TestValueObject<TValue>>,
      System.IComparable
{
    public int CompareTo(TestValueObject<TValue>? other)
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

        return obj is TestValueObject<TValue> other ? CompareTo(other) : throw new System.ArgumentException($""Object must be of type {nameof(TestValueObject<TValue>)}"");
    }
}
";

        await Verify.VerifyGeneratorAsync(source,
                                          ("SomeNamespace.TestValueObject.CastOperators.g.cs", generatedCasts),
                                          ("SomeNamespace.TestValueObject.IComparable.g.cs", generatedComparable));
    }

    [Fact]
    public async Task AbstractValueObject_GeneratesCorrectCode()
    {
        var source = @"
using Cranks.SeedWork.Domain;
using System;

namespace SomeNamespace;

[ValueObject]
public abstract partial record TestValueObject<TValue>(TValue Value) : ValueObject
    where TValue : IComparable<TValue>;
";
        var generatedCasts = @"// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SomeNamespace;

partial record TestValueObject<TValue>
{
    public static implicit operator TValue(TestValueObject<TValue> value)
    {
        return value.Value;
    }
}
";

        var generatedComparable = @"// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SomeNamespace;

partial record TestValueObject<TValue>
    : System.IComparable<SomeNamespace.TestValueObject<TValue>>,
      System.IComparable
{
    public int CompareTo(TestValueObject<TValue>? other)
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

        return obj is TestValueObject<TValue> other ? CompareTo(other) : throw new System.ArgumentException($""Object must be of type {nameof(TestValueObject<TValue>)}"");
    }
}
";

        await Verify.VerifyGeneratorAsync(source,
                                          ("SomeNamespace.TestValueObject.CastOperators.g.cs", generatedCasts),
                                          ("SomeNamespace.TestValueObject.IComparable.g.cs", generatedComparable));
    }
}
