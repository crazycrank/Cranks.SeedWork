// ReSharper disable InconsistentNaming

#pragma warning disable SA1310 // Field names should not contain underscore

using Microsoft.CodeAnalysis;

namespace Cranks.SeedWork.Domain.Generator.Analyzers;

internal static partial class Rules
{
    public static readonly DiagnosticDescriptor SmartEnums_MustNotBeInstantiated
        = new("SEED0101",
              "SmartEnums must not be instantiated outside their defining class",
              "The smart object '{0}' directly instantiated outside it's definining class. This is not supported.",
              Categories.Domain,
              DiagnosticSeverity.Error,
              true);
}
