using System.Text;

using Cranks.SeedWork.Domain.Analyzers.Test.Verifiers.Base;
using Cranks.SeedWork.Domain.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

namespace Cranks.SeedWork.Domain.Analyzers.Test.Verifiers;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
#pragma warning disable CA1000 // Do not declare static members on generic types

    public static async Task VerifyGeneratorAsync(string source, params (string Filename, string Source)[] generated)
    {
        var test = new Test(source, generated);

        await test.RunAsync(CancellationToken.None);
    }

#pragma warning restore CA1000 // Do not declare static members on generic types

    private class Test : CSharpIncrementalGeneratorTest<TSourceGenerator, XUnitVerifier>
    {
        public Test(string source, params (string Filename, string Source)[] generated)
        {
            TestState.Sources.Add(source);

            foreach (var (filename, generatedSource) in generated)
            {
                TestState.GeneratedSources.Add((typeof(TSourceGenerator),
                                                filename,
                                                SourceText.From(generatedSource, Encoding.UTF8)));
            }

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

        public LanguageVersion LanguageVersion { get; } = LanguageVersion.Default;

        protected override CompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = base.CreateCompilationOptions();
            return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
        }

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
        }
    }
}
