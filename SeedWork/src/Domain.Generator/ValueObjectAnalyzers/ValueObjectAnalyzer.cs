using System.Collections.Immutable;

using Cranks.SeedWork.Domain.Generator.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValueObjectAnalyzer : DiagnosticAnalyzer
{
    public const string MustBePartialDiagnosticId = "SEED0001";
    public const string MustBeRecordDiagnosticId = "SEED0002";
    public const string MustDeriveFromValueObjectDiagnosticId = "SEED0003";

    public static readonly DiagnosticDescriptor MustBePartialRule = new(MustBePartialDiagnosticId,
                                                                        "ValueObject must be partial",
                                                                        "The value object '{0}' is not partial. Value objects must be marked as partial.",
                                                                        "SeedWork Domain",
                                                                        DiagnosticSeverity.Error,
                                                                        true);

    public static readonly DiagnosticDescriptor MustBeRecordRule = new(MustBeRecordDiagnosticId,
                                                                       "ValueObject must be a record",
                                                                       "The value object '{0}' is not a record. Value objects must be records.",
                                                                       "SeedWork Domain",
                                                                       DiagnosticSeverity.Error,
                                                                       true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(MustBePartialRule,
                                MustBeRecordRule);

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
            if (dcs.GetSyntax() is not TypeDeclarationSyntax typeDeclarationSyntax)
            {
                continue;
            }

            if (typeDeclarationSyntax is not RecordDeclarationSyntax)
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(MustBeRecordRule,
                                                   typeDeclarationSyntax.Identifier.GetLocation(),
                                                   type.Name);

                context.ReportDiagnostic(diagnostic);
            }

            if (!typeDeclarationSyntax.IsPartial())
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(MustBePartialRule,
                                                   typeDeclarationSyntax.Identifier.GetLocation(),
                                                   type.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
