using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Cranks.SeedWork.Domain.Generator.Tests.Verifiers.Base;

/// <summary>
///     From https://github.com/dotnet/roslyn-sdk/issues/940#issuecomment-1096993307
/// </summary>
public class CSharpIncrementalGeneratorTest<TSourceGenerator, TVerifier> : SourceGeneratorTest<TVerifier>
    where TSourceGenerator : IIncrementalGenerator, new()
    where TVerifier : IVerifier, new()
{
    public override string Language => LanguageNames.CSharp;

    protected override string DefaultFileExt => "cs";

    protected override IEnumerable<ISourceGenerator> GetSourceGenerators()
    {
        return new[] { new TSourceGenerator().AsSourceGenerator() };
    }

    protected override GeneratorDriver CreateGeneratorDriver(Project project, ImmutableArray<ISourceGenerator> sourceGenerators)
    {
        return CSharpGeneratorDriver.Create(
            sourceGenerators,
            project.AnalyzerOptions.AdditionalFiles,
            (CSharpParseOptions)project.ParseOptions!,
            project.AnalyzerOptions.AnalyzerConfigOptionsProvider);
    }

    protected override CompilationOptions CreateCompilationOptions()
    {
        return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
    }

    protected override ParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(LanguageVersion.Default, DocumentationMode.Diagnose);
    }
}
