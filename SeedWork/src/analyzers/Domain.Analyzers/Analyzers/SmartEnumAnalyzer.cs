using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cranks.SeedWork.Domain.Generator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SmartEnumAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create<DiagnosticDescriptor>();

    public override void Initialize(AnalysisContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        { // uncomment to debug during dotnet build
////#warning DO NOT CHECKIN
////            Debugger.Launch();
        }
#endif
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context)
    {
    }
}
