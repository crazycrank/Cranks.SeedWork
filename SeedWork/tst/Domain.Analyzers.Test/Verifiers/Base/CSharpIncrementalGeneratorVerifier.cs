using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Cranks.SeedWork.Domain.Analyzers.Test.Verifiers.Base;

/// <summary>
///     From https://github.com/dotnet/roslyn-sdk/issues/940#issuecomment-1096993307
/// </summary>
public class CSharpIncrementalGeneratorVerifier<TSourceGenerator, TVerifier> : IncrementalGeneratorVerifier<TSourceGenerator,
    CSharpIncrementalGeneratorTest<TSourceGenerator, TVerifier>, TVerifier>
    where TSourceGenerator : IIncrementalGenerator, new()
    where TVerifier : IVerifier, new()
{
}
