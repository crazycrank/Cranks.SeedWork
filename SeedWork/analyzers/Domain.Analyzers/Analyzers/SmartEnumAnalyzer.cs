using System.Collections.Immutable;

using Cranks.SeedWork.Domain.Analyzers.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cranks.SeedWork.Domain.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SmartEnumAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rules.SmartEnum_MustBeRecord,
                                 Rules.SmartEnum_MustBePartial,
                                 Rules.SmartEnum_MustDeriveFromSmartEnum,
                                 Rules.SmartEnum_MustBeSealed,
                                 Rules.SmartEnum_MustNotDeriveFromNonGenericSmartEnum,
                                 Rules.SmartEnum_ShouldNotBeNested);

    public override void Initialize(AnalysisContext context)
    {
#if LAUNCH_DEBUGGER
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context)
    {
        // type symbol must be marked with SmartEnumAttribute
        if (context.Symbol is not INamedTypeSymbol type || !type.IsMarkedAsSmartEnum())
        {
            return;
        }

        // symbol must be a type declaration
        if (type.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken) is not TypeDeclarationSyntax tds)
        {
            return;
        }

        // symbol must also be a record declaration
        if (tds is not RecordDeclarationSyntax rds)
        {
            var diagnostic = Diagnostic.Create(Rules.SmartEnum_MustBeRecord,
                                               tds.Identifier.GetLocation(),
                                               type.Name);

            context.ReportDiagnostic(diagnostic);
            return;
        }

        // TODO this should in theory provide if the syntax tree is auto generated, but always returns Unknown
        ////context.Compilation.Options.SyntaxTreeOptionsProvider.IsGenerated(type.DeclaringSyntaxReferences[1].GetSyntax().SyntaxTree, context.CancellationToken);

        // there should not be any partial implementation, as this interferes with other rules and code gen
        ////if (type.DeclaringSyntaxReferences.Count(dcs => dcs.GetSyntax().SyntaxTree.FilePath.EndsWith(".g.cs", StringComparison.Ordinal)) > 1)
        ////{
        ////    var diagnostic = Diagnostic.Create(Rules.SmartEnum_MustNotHavePartialImplementation,
        ////                                       tds.Identifier.GetLocation(),
        ////                                       type.Name);

        ////    context.ReportDiagnostic(diagnostic);
        ////    return;
        ////}

        // record should be marked as partial
        if (!rds.IsPartial())
        {
            var diagnostic = Diagnostic.Create(Rules.SmartEnum_MustBePartial,
                                               rds.Identifier.GetLocation(),
                                               type.Name);

            context.ReportDiagnostic(diagnostic);
        }

        if (!type.IsSealed)
        {
            var diagnostic = Diagnostic.Create(Rules.SmartEnum_MustBeSealed,
                                               rds.Identifier.GetLocation(),
                                               type.Name);

            context.ReportDiagnostic(diagnostic);
        }

        // record should derive from SmartEnum<T>
        if (type.BaseType!.SpecialType == SpecialType.System_Object
            || (!type.BaseType.IsSmartEnumGenericBaseClass() && !type.BaseType.IsSmartEnumBaseClass()))
        {
            // For all such symbols, produce a diagnostic.
            var diagnostic = Diagnostic.Create(Rules.SmartEnum_MustDeriveFromSmartEnum,
                                               rds.Identifier.GetLocation(),
                                               type.Name);

            context.ReportDiagnostic(diagnostic);
        }

        // record should not derive from non generic base class
        if (type.BaseType.IsSmartEnumBaseClass())
        {
            // For all such symbols, produce a diagnostic.
            var diagnostic = Diagnostic.Create(Rules.SmartEnum_MustNotDeriveFromNonGenericSmartEnum,
                                               rds.Identifier.GetLocation(),
                                               type.Name);

            context.ReportDiagnostic(diagnostic);
        }

        // should not be a nested definition
        if (rds.IsNestedTypeDeclaration())
        {
            // For all such symbols, produce a diagnostic.
            var diagnostic = Diagnostic.Create(Rules.SmartEnum_ShouldNotBeNested,
                                               rds.Identifier.GetLocation(),
                                               type.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
