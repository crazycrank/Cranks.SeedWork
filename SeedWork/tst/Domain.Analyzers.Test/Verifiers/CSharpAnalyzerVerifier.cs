using Cranks.SeedWork.Domain.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Cranks.SeedWork.Domain.Analyzers.Test.Verifiers;

public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
#pragma warning disable CA1000 // Do not declare static members on generic types
    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()" />
    public static DiagnosticResult Diagnostic()
    {
        return CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic();
    }

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)" />
    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);
    }

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)" />
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
    {
        return CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(descriptor);
    }

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])" />
    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test(source, expected);

        await test.RunAsync(CancellationToken.None);
    }
#pragma warning restore CA1000 // Do not declare static members on generic types

    private class Test : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
    {
        public Test(string source,
                    params DiagnosticResult[] expected)
        {
            TestCode = source;
            ExpectedDiagnostics.AddRange(expected);

            ReferenceAssemblies = new ReferenceAssemblies("net6.0",
                                                          new PackageIdentity("Microsoft.NETCore.App.Ref", "6.0.0"),
                                                          Path.Combine("ref", "net6.0"));

            TestState.AdditionalReferences.Add(typeof(ValueObjectAttribute).Assembly);

            SolutionTransforms.Add((solution, projectId) =>
                                   {
                                       var compilationOptions = solution.GetProject(projectId)!.CompilationOptions;
                                       compilationOptions = compilationOptions!.WithSpecificDiagnosticOptions(
                                           compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));

                                       solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                                       return solution;
                                   });
        }
    }
}
