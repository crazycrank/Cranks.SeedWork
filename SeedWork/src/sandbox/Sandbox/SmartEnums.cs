using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Cranks.SeedWork.Domain;

namespace Cranks.SeedWork.Sandbox;

[SmartEnum]
public sealed record Gender(string Key) : SmartEnum<string>(Key)
{
    public static readonly Gender Unknown = new("unknown");
    public static readonly Gender Male = new("male");
    public static readonly Gender Female = new("female");

    private static ImmutableList<Gender>? _allValues;

    private static ImmutableList<Gender> GetAllValues()
    {
        var builder = ImmutableList.CreateBuilder<Gender>();
        builder.Add(Unknown);
        builder.Add(Male);
        builder.Add(Female);
        return builder.ToImmutable();
    }

    public static ImmutableList<Gender> AllValues => _allValues ??= GetAllValues();

    public static bool TryGet(string key, [NotNullWhen(true)] out Gender? value)
    {
        value = AllValues.SingleOrDefault(v => v.Key == key);
        return value is not null;
    }

    public static Gender Get(string key)
    {
        if (TryGet(key, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"No {nameof(Gender)} with key {key} exists");
    }
}

[SmartEnum]
public record Gender2(int Key, string Name) : SmartEnum<int>(Key)
{
    public static readonly Gender2 Unknown = new(0, "unknown");
    public static readonly Gender2 Male = new(1, "male");
    public static readonly Gender2 Female = new(2, "female");
}

[SmartEnum]
public record AddressType(string Key, int Type) : SmartEnum<string>(Key)
{
    public static readonly AddressType Home = new("home", 1234);
    public static readonly AddressType Work = new("work", 7777);
}

[SmartEnum]
public record MagicType(string Key) : SmartEnum<string>(Key)
{
    public static readonly MagicType Arcane = new(nameof(Arcane));
    public static readonly MagicType Necormancy = new(nameof(Necormancy));
}

[SmartEnum]
public record Spell(string Key, int BaseDamage, MagicType Type) : SmartEnum<string>(Key)
{
    public static readonly Spell Bolt = new(nameof(Bolt), 10, MagicType.Arcane);
    public static readonly Spell Bite = new(nameof(Bite), 5, MagicType.Necormancy);
}
