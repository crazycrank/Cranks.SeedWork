using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Cranks.SeedWork.Domain.Generator.Tests.Verifiers;

#pragma warning disable CA1000 // Do not declare static members on generic types
public static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic()" />
    public static DiagnosticResult Diagnostic()
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, XUnitVerifier>.Diagnostic();
    }

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)" />
    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, XUnitVerifier>.Diagnostic(diagnosticId);
    }

    /// <inheritdoc cref="Microsoft.CodeAnalysis.Diagnostic" />
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, XUnitVerifier>.Diagnostic(descriptor);
    }

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])" />
    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test(source, expected);
        await test.RunAsync(CancellationToken.None);
    }

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)" />
    public static async Task VerifyCodeFixAsync(string source, string fixedSource)
    {
        await VerifyCodeFixAsync(source, fixedSource, DiagnosticResult.EmptyDiagnosticResults);
    }

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)" />
    public static async Task VerifyCodeFixAsync(string source,
                                                string fixedSource,
                                                string residualDiagnostic,
                                                params DiagnosticResult[] expected)
    {
        var test = new Test(source, fixedSource, expected)
                   {
                       DisabledDiagnostics = { residualDiagnostic },
                   };

        await test.RunAsync(CancellationToken.None);
    }

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)" />
    public static async Task VerifyCodeFixAsync(string source,
                                                string fixedSource,
                                                params DiagnosticResult[] expected)
    {
        var test = new Test(source, fixedSource, expected);

        await test.RunAsync(CancellationToken.None);
    }

    private class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
    {
        public Test(string source,
                    string fixedSource,
                    params DiagnosticResult[] expected)
            : this()
        {
            TestCode = source;
            FixedCode = fixedSource;
            ExpectedDiagnostics.AddRange(expected);
        }

        public Test(string source,
                    params DiagnosticResult[] expected)
            : this()
        {
            TestCode = source;
            ExpectedDiagnostics.AddRange(expected);
        }

        private Test()
        {
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
