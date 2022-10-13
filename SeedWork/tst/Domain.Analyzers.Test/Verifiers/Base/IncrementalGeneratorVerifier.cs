using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Cranks.SeedWork.Domain.Analyzers.Test.Verifiers.Base;

/// <summary>
///     A default verifier for incremental source generators.
///     From https://github.com/dotnet/roslyn-sdk/issues/940#issuecomment-1096993307
/// </summary>
public class IncrementalGeneratorVerifier<TSourceGenerator, TTest, TVerifier>
    where TSourceGenerator : IIncrementalGenerator, new()
    where TTest : SourceGeneratorTest<TVerifier>, new()
    where TVerifier : IVerifier, new()
{
}
