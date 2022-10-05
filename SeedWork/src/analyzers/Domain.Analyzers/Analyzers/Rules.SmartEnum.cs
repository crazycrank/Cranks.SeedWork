// ReSharper disable InconsistentNaming

#pragma warning disable SA1310 // Field names should not contain underscore

using Microsoft.CodeAnalysis;

namespace Cranks.SeedWork.Domain.Generator.Analyzers;

internal static partial class Rules
{
    public static readonly DiagnosticDescriptor SmartEnum_MustBePartial
        = new("SEED0101",
              "SmartEnum must be partial",
              "The smart enum '{0}' is not partial. Value objects must be marked as partial.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnum_MustBeRecord
        = new("SEED0102",
              "SmartEnum must be a record",
              "The smart enum '{0}' is not a record. Value objects must be records.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnum_MustDeriveFromSmartEnum
        = new("SEED0103",
              "SmartEnum must derive from SmartEnum<T>",
              "The smart enum '{0}' does not inherit from SmartEnum<T>. Value objects must derive from SmartEnum<>.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnum_MustHavePrimaryConstructor
        = new("SEED0104",
              "SmartEnum must have a primary constructor",
              "The smart enum '{0}' is expected to have a primary constructor, but it could not be found.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnum_ShouldNotBeNested
        = new("SEED0105",
              "SmartEnum must not be a nested class",
              "The smart enum '{0}' is nested inside another class. This is not supported",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnum_MustNotDeriveFromNonGenericSmartEnum
        = new("SEED0106",
              "Do not derive from non-generic SmartEnum",
              "The smart enum '{0}' derives from the non-generic SmartEnum base class, which is non intended to be derived from directly",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnum_MustNotHavePartialImplementation
        = new("SEED0107",
              "SmartEnum should not have multiple partial declarations",
              "The smart enum '{0}' has more than one partial implementation. This can lead to errors in code generation",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnums_MustNotBeInstantiated
        = new("SEED0108",
              "SmartEnums must not be instantiated outside their defining class",
              "The smart enum '{0}' directly instantiated outside it's definining class. This is not supported.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnums_DuplicateKey
        = new("SEED0109",
              "SmartEnum contains duplicate key",
              "The smart enum '{0}' contains duplicates for the same key.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnums_DoNotUseWithExpression
        = new("SEED0110",
              "Do not use with expression on SmartEnum",
              "The smart enum '{0}' contains duplicates for the same key.", // TODO
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnums_ShouldBeSealed
        = new("SEED0111",
              "SmartEnum should be sealed",
              "The smart enum '{0}' contains duplicates for the same key.", // TODO
              Categories.Domain,
              DiagnosticSeverity.Warning,
              true);
}
