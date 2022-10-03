using Cranks.SeedWork.Domain.Generator.Analyzers;

using Verify = Cranks.SeedWork.Domain.Generator.Tests.Verifiers.CSharpCodeFixVerifier<
    Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers.ValueObjectAnalyzer,
    Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers.ValueObjectAnalyzerCodeFixProvider>;

namespace Cranks.SeedWork.Domain.Generator.Tests.ValueObjectAnalyzers;

public class ValueObjectAnalyzerTest
{
    [Fact]
    public async Task EmptyCode_LeadsToNoDiagnostics()
    {
        var test = string.Empty;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidCode_NoDiagnostics()
    {
        var test = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record TestValueObject(int Value) : ValueObject<TestValueObject>;
";

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NotPartial_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public record {|#0:TestValueObject|}(int Value) : ValueObject<TestValueObject>;
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : ValueObject<TestValueObject>;
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustBePartial).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, expected);
    }

    [Fact]
    public async Task NotARecord_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial class {|#0:TestValueObject|}(int Value)
    {
    }
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value)
    {
    }
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustBeRecord.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, expected);
    }

    [Fact]
    public async Task NoBaseClass_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value);
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : ValueObject<TestValueObject>;
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustDeriveFromValueObject.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, expected);
    }

    [Fact]
    public async Task WrongBaseClass_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    public class BaseClass {}

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : BaseClass;
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain;

    public class BaseClass {}

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : ValueObject<TestValueObject>;
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustDeriveFromValueObject.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, expected);
    }

    [Fact]
    public async Task BaseInterface_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    public interface IInterface {}

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : IInterface;
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain;

    public interface IInterface {}

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : ValueObject<TestValueObject>, IInterface;
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustDeriveFromValueObject.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, expected);
    }
}
