// ReSharper disable InconsistentNaming

#pragma warning disable SA1310 // Field names should not contain underscore

using Microsoft.CodeAnalysis;

namespace Cranks.SeedWork.Domain.Analyzers.Analyzers;

internal static partial class Rules
{
    public static readonly DiagnosticDescriptor ValueObject_MustBePartial
        = new("SEED0001",
              "ValueObject must be partial",
              "The value object '{0}' is not partial. Value objects must be marked as partial.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor ValueObject_MustBeRecord
        = new("SEED0002",
              "ValueObject must be a record",
              "The value object '{0}' is not a record. Value objects must be records.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor ValueObject_MustDeriveFromValueObject
        = new("SEED0003",
              "ValueObject must derive from ValueObject",
              "The value object '{0}' does not inherit from ValueObject. Value objects must derive from ValueObject.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor ValueObject_MustHavePrimaryConstructor
        = new("SEED0004",
              "ValueObject must have a primary constructor",
              "The value object '{0}' is expected to have a primary constructor, but it could not be found.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor ValueObject_ShouldNotBeNested
        = new("SEED0005",
              "ValueObject must not be a nested class",
              "The value object '{0}' is nested inside another class. This is not supported",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor ValueObject_MustNotHavePartialImplementation
        = new("SEED0007",
              "ValueObject should not have multiple partial declarations",
              "The value object '{0}' has more than one partial implementation. This can lead to errors in code generation",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor ValueObject_MustHaveAtLeastOneParameter
        = new("SEED0008",
              "Primary constructor is expected to have at least one parameter",
              "The primary constructor for '{0}' is expected to have at least one parameter",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor ValueObject_AllParametersShouldBeEquatable
        = new("SEED0009",
              "ValueObject parameters should be equatable",
              "The value object '{0}' should only have parameters which are equatable",
              Categories.Domain,
              DiagnosticSeverity.Warning,
              true);
}
