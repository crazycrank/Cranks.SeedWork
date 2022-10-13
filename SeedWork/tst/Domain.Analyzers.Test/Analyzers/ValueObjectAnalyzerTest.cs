using Cranks.SeedWork.Domain.Analyzers.Analyzers;

using Verify = Cranks.SeedWork.Domain.Analyzers.Test.Verifiers.CSharpCodeFixVerifier<
    Cranks.SeedWork.Domain.Analyzers.Analyzers.ValueObjectAnalyzer,
    Cranks.SeedWork.Domain.Analyzers.Analyzers.ValueObjectAnalyzerCodeFixProvider>;

namespace Cranks.SeedWork.Domain.Analyzers.Test.Analyzers;

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
    public partial class {|#0:TestValueObject|}
    {
    }
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record {|#0:TestValueObject|}
    {
    }
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustBeRecord.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, Rules.ValueObject_MustDeriveFromValueObject.Id, expected);
    }

    [Fact]
    public async Task NoBaseClass_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value);
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustDeriveFromValueObject.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task NonGenericBaseClass_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : ValueObject;
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustNotDeriveFromNonGenericValueObject.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task WrongBaseClass__ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    public record BaseClass {}

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : BaseClass;
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustDeriveFromValueObject.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task BaseInterface_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    public interface IInterface {}

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : IInterface;
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustDeriveFromValueObject.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact(Skip = "Cannot exclude generated code from analysis")]
    public async Task MultipleImplementations_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [ValueObject]
    public partial record {|#0:TestValueObject|}(int Value) : ValueObject<TestValueObject>;
    public partial record TestValueObject;
";

        var expected = Verify.Diagnostic(Rules.ValueObject_MustNotHavePartialImplementation.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task NestedDeclaration_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    public class Parent {
        [ValueObject]
        public partial record {|#0:TestValueObject|}(int Value) : ValueObject<TestValueObject>;
    }
";

        var expected = Verify.Diagnostic(Rules.ValueObject_ShouldNotBeNested.Id).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }
}
