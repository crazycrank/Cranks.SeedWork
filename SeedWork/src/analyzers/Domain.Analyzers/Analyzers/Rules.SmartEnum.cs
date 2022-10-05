// ReSharper disable InconsistentNaming

#pragma warning disable SA1310 // Field names should not contain underscore

using Microsoft.CodeAnalysis;

namespace Cranks.SeedWork.Domain.Generator.Analyzers;

internal static partial class Rules
{
    public static readonly DiagnosticDescriptor SmartEnums_MustNotBeInstantiated
        = new("SEED0101",
              "SmartEnums must not be instantiated outside their defining class",
              "The smart enum '{0}' directly instantiated outside it's definining class. This is not supported.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnums_DuplicateKey
        = new("SEED0102",
              "SmartEnum contains duplicate key",
              "The smart enum '{0}' contains duplicates for the same key.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnums_DoNotUseWith
        = new("SEED0103",
              "Do not use with expression on smart enums",
              "The smart enum '{0}' contains duplicates for the same key.", // TODO
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);

    public static readonly DiagnosticDescriptor SmartEnums_ShouldBeSealed
        = new("SEED0103",
              "Smart enums should be sealed",
              "The smart enum '{0}' contains duplicates for the same key.", // TODO
              Categories.Domain,
              DiagnosticSeverity.Warning,
              true);
}
