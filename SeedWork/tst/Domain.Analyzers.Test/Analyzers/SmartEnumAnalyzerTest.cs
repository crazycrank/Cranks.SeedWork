using Cranks.SeedWork.Domain.Generator.Analyzers;

using Verify = Cranks.SeedWork.Domain.Generator.Tests.Verifiers.CSharpCodeFixVerifier<
    Cranks.SeedWork.Domain.Generator.Analyzers.SmartEnumAnalyzer,
    Cranks.SeedWork.Domain.Generator.Analyzers.SmartEnumAnalyzerCodeFixProvider>;

namespace Cranks.SeedWork.Domain.Generator.Tests.Analyzers;

public class SmartEnumAnalyzerTest
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

    [SmartEnum]
    public sealed partial record TestSmartEnum(int Key) : SmartEnum<int>(Key);
";

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NotPartial_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [SmartEnum]
    public sealed record {|#0:TestSmartEnum|}(int Key) : SmartEnum<int>(Key);
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain;

    [SmartEnum]
    public sealed partial record {|#0:TestSmartEnum|}(int Key) : SmartEnum<int>(Key);
";

        var expected = Verify.Diagnostic(Rules.SmartEnum_MustBePartial).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, expected);
    }

    [Fact]
    public async Task NotARecord_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [SmartEnum]
    public sealed partial class {|#0:TestSmartEnum|}
    {
    }
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain;

    [SmartEnum]
    public sealed partial record {|#0:TestSmartEnum|}
    {
    }
";

        var expected = Verify.Diagnostic(Rules.SmartEnum_MustBeRecord.Id).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, Rules.SmartEnum_MustDeriveFromSmartEnum.Id, expected);
    }

    [Fact]
    public async Task NoBaseClass_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [SmartEnum]
    public sealed partial record {|#0:TestSmartEnum|}(int Key);
";

        var expected = Verify.Diagnostic(Rules.SmartEnum_MustDeriveFromSmartEnum.Id).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task NonGenericBaseClass_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [SmartEnum]
    public sealed partial record {|#0:TestSmartEnum|}(int Key) : SmartEnum;
";

        var expected = Verify.Diagnostic(Rules.SmartEnum_MustNotDeriveFromNonGenericSmartEnum.Id).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task WrongBaseClass_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    public record BaseClass;

    [SmartEnum]
    public sealed partial record {|#0:TestSmartEnum|}(int Key) : BaseClass;
";

        var expected = Verify.Diagnostic(Rules.SmartEnum_MustDeriveFromSmartEnum.Id).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task BaseInterface_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    public interface IInterface {}

    [SmartEnum]
    public sealed partial record {|#0:TestSmartEnum|}(int Key) : IInterface;
";

        var expected = Verify.Diagnostic(Rules.SmartEnum_MustDeriveFromSmartEnum.Id).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact(Skip = "Cannot exclude generated code from analysis")]
    public async Task MultipleImplementations_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [SmartEnum]
    public sealed partial record {|#0:TestSmartEnum|}(int Key) : SmartEnum<int>(Key);
    public sealed partial record TestSmartEnum;
";

        var expected = Verify.Diagnostic(Rules.SmartEnum_MustNotHavePartialImplementation.Id).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task NestedDeclaration_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    public class Parent
    {
        [SmartEnum]
        public sealed partial record {|#0:TestSmartEnum|}(int Key) : SmartEnum<int>(Key);
    }
";

        var expected = Verify.Diagnostic(Rules.SmartEnum_ShouldNotBeNested.Id).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task NotSealed_ReportsDiagnostic()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain;

    [SmartEnum]
    public partial record {|#0:TestSmartEnum|}(int Key) : SmartEnum<int>(Key);
";

        var expected = Verify.Diagnostic(Rules.SmartEnums_MustBeSealed.Id).WithLocation(0).WithArguments("TestSmartEnum");
        await Verify.VerifyAnalyzerAsync(testCode, expected);
    }
}
