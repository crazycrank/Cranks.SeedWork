////namespace Cranks.SeedWork.Domain;

////public abstract record ValueObjectComparable<T>(T Value)
////    : ValueObject<T>(Value),
////        IComparable<ValueObjectComparable<T>>,
////        IComparable
////    where T : IEquatable<T>, IComparable<T>
////{
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

////        return obj is ValueObjectComparable<T> other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ValueObjectComparable<T>)}");
////    }

////    public int CompareTo(ValueObjectComparable<T>? other)
////    {
////        if (ReferenceEquals(null, other))
////        {
////            return 1;
////        }

////        return Value.CompareTo(other.Value);
////    }

////    public static bool operator <(ValueObjectComparable<T>? left, ValueObjectComparable<T>? right)
////    {
////        return Comparer<ValueObjectComparable<T>>.Default.Compare(left, right) < 0;
////    }

////    public static bool operator >(ValueObjectComparable<T>? left, ValueObjectComparable<T>? right)
////    {
////        return Comparer<ValueObjectComparable<T>>.Default.Compare(left, right) > 0;
////    }

////    public static bool operator <=(ValueObjectComparable<T>? left, ValueObjectComparable<T>? right)
////    {
////        return Comparer<ValueObjectComparable<T>>.Default.Compare(left, right) <= 0;
////    }

////    public static bool operator >=(ValueObjectComparable<T>? left, ValueObjectComparable<T>? right)
////    {
////        return Comparer<ValueObjectComparable<T>>.Default.Compare(left, right) >= 0;
////    }
////}
