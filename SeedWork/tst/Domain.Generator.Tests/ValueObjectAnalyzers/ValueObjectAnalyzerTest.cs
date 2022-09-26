using Cranks.SeedWork.Domain.Generator.ValueObjectAnalyzers;

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
    using Cranks.SeedWork.Domain.Attributes;

    [ValueObject]
    public partial record TestValueObject;
";

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NotPartial_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain.Attributes;

    [ValueObject]
    public record {|#0:TestValueObject|};
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain.Attributes;

    [ValueObject]
    public partial record {|#0:TestValueObject|};
";

        var expected = Verify.Diagnostic(ValueObjectAnalyzer.MustBePartialDiagnosticId).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, expected);
    }

    [Fact]
    public async Task NotARecord_CodeFixWorks()
    {
        var testCode = @"
    using Cranks.SeedWork.Domain.Attributes;

    [ValueObject]
    public partial class {|#0:TestValueObject|}
    {
    }
";

        var testCodeExpected = @"
    using Cranks.SeedWork.Domain.Attributes;

    [ValueObject]
    public partial record {|#0:TestValueObject|}
    {
    }
";

        var expected = Verify.Diagnostic(ValueObjectAnalyzer.MustBeRecordDiagnosticId).WithLocation(0).WithArguments("TestValueObject");
        await Verify.VerifyCodeFixAsync(testCode, testCodeExpected, expected);
    }

////    //Diagnostic and CodeFix both triggered and checked for
////    [Fact]
////    public async Task InvalidCode_GetsFixed()
////    {
////        var test = @"
////    using System;
////    using System.Collections.Generic;
////    using System.Linq;
////    using System.Text;
////    using System.Threading.Tasks;
////    using System.Diagnostics;
////    using Cranks.SeedWork.Domain;

////    [ValueObject]
////    public record |#0:TestRecord|;
////";

////        var fixtest = @"
////    using System;
////    using System.Collections.Generic;
////    using System.Linq;
////    using System.Text;
////    using System.Threading.Tasks;
////    using System.Diagnostics;
////    using Cranks.SeedWork.Domain;

////    [ValueObject]
////    public partial record TestRecord;
////";

////        var expected = Verify.Diagnostic(ValueObjectAnalyzer.MustBePartialDiagnosticId).WithLocation(0).WithArguments("TypeName");
////        await Verify.VerifyCodeFixAsync(test, expected, fixtest);
////    }
}
