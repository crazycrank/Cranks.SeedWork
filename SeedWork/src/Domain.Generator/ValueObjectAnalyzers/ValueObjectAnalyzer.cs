using System.Collections.Immutable;

using Cranks.SeedWork.Domain.Generator.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValueObjectAnalyzer : DiagnosticAnalyzer
{
    public const string MustBePartialId = "SEED0001";
    public const string MustBeRecordId = "SEED0002";
    public const string MustDeriveFromValueObjectId = "SEED0003";

    public static readonly DiagnosticDescriptor MustBePartial = new(MustBePartialId,
                                                                    "ValueObject must be partial",
                                                                    "The value object '{0}' is not partial. Value objects must be marked as partial.",
                                                                    "SeedWork Domain",
                                                                    DiagnosticSeverity.Error,
                                                                    true);

    public static readonly DiagnosticDescriptor MustBeRecord = new(MustBeRecordId,
                                                                   "ValueObject must be a record",
                                                                   "The value object '{0}' is not a record. Value objects must be records.",
                                                                   "SeedWork Domain",
                                                                   DiagnosticSeverity.Error,
                                                                   true);

    public static readonly DiagnosticDescriptor MustDeriveFromValueObject = new(MustDeriveFromValueObjectId,
                                                                                "ValueObject must derive from ValueObject<>",
                                                                                "The value object '{0}' does not inherit from ValueObject<>. Value objects must derive from ValueObject<>.",
                                                                                "SeedWork Domain",
                                                                                DiagnosticSeverity.Error,
                                                                                true);

    // TODO MustHaveAtLeastOneParameter
    // TODO Must not have partial implementations
    // TODO Parameters should be Equatable
    // TODO ValueObjects should not be nested
    // TODO ValueObject must derive from ValueObject<>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(MustBePartial,
                                 MustBeRecord,
                                 MustDeriveFromValueObject);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context)
    {
        if (!context.Symbol.IsValueObject())
        {
            return;
        }

        var type = (INamedTypeSymbol)context.Symbol;

        foreach (var dcs in type.DeclaringSyntaxReferences)
        {
            if (dcs.GetSyntax(context.CancellationToken) is not TypeDeclarationSyntax nts)
            {
                continue;
            }

            if (nts is not RecordDeclarationSyntax rds)
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(MustBeRecord,
                                                   nts.Identifier.GetLocation(),
                                                   type.Name);

                context.ReportDiagnostic(diagnostic);
                continue;
            }

            if (!rds.IsPartial())
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(MustBePartial,
                                                   rds.Identifier.GetLocation(),
                                                   type.Name);

                context.ReportDiagnostic(diagnostic);
            }

            // when no base list or none of the base types derives from ValueObject
            if (!DerivesFromValueObject(context, rds))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(MustDeriveFromValueObject,
                                                   rds.Identifier.GetLocation(),
                                                   type.Name);

                context.ReportDiagnostic(diagnostic);
            }

            // TODO check at least one parameter
            if (false)
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(MustDeriveFromValueObject,
                                                   rds.Identifier.GetLocation(),
                                                   type.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool DerivesFromValueObject(SymbolAnalysisContext context, RecordDeclarationSyntax rds)
    {
        return rds.BaseList?.Types.Any(t =>
                                       {
                                           var baseTypeName = t.Type.ExtractTypeName();
                                           if (baseTypeName is null)
                                           {
                                               return false;
                                           }

                                           return context.Compilation
                                                         .GetSymbolsWithName(baseTypeName,
                                                                             SymbolFilter.Type,
                                                                             context.CancellationToken)
                                                         .Any(s => s is INamedTypeSymbol
                                                                        {
                                                                            ContainingNamespace.Name: "Cranks.SeedWork.Domain",
                                                                            Name: "ValueObject",
                                                                        });
                                       })
               ?? false;
    }
}
