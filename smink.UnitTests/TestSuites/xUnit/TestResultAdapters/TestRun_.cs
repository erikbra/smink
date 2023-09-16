using FluentAssertions;
using smink.Models.Report;
using smink.TestResultAdapters;
using smink.TestResultReaders;

// ReSharper disable InconsistentNaming

namespace smink.UnitTests.TestSuites.xUnit.TestResultAdapters;

[Collection(nameof(RunXUnitTestsFixture))]
public class TestRun_
{
    private readonly TestRun _passedTest;
    private readonly TestRun _skippedTest;
    private readonly TestRun _failedTest;
    private readonly RunXUnitTestsFixture _fixture;

    public TestRun_(RunXUnitTestsFixture fixture)
    {
        var report = fixture.TestReport;

        _fixture = fixture;

        var testSuite = report!.TestSuites.SingleOrDefault(s => "Adding a new customer".Equals(s.DisplayName));
        TestScenario[] scenarios = testSuite!.TestScenarios.ToArray();

        var withSkipped = scenarios[0];
        var withFailed = scenarios[1];

        _passedTest = withSkipped.Tests.First();
        _skippedTest = withSkipped.Tests.Skip(1).First();
        _failedTest = withFailed.Tests.First();
    }

    [Fact]
    public void Has_correct_Name() => _passedTest.Name.Should().Be("xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed.We_validate_the_mobile_number_of_the_customer");

    [Fact]
    public void Has_correct_DisplayName() => _passedTest.DisplayName.Should().Be("We validate the mobile number of the customer");
    
    [Fact]
    public void Has_correct_Type() => _passedTest.Type.Should().Be("xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed");
    
    [Fact]
    public void Has_correct_Method() => _passedTest.Method.Should().Be("We_validate_the_mobile_number_of_the_customer");
    
    [Fact]
    public void Passed_test_has_correct_Result() => _passedTest.Result.Should().Be("Pass");
    
    [Fact]
    public void Skipped_test_has_correct_Result() => _skippedTest.Result.Should().Be("Skip");
    
    [Fact]
    public void Skipped_test_has_correct_Reason() => _skippedTest.Reason.Should().Be("This might be too expensive");
    
    [Fact(Skip = "Xunit reporter 3.1.* -> doesn't fill out the Output, only the Reason")]
    public void Skipped_test_has_correct_Output() => _skippedTest.Output.Should().Be(@"This might be too expensive
");

    [Fact]
    public void Failed_test_has_correct_Result() => _failedTest.Result.Should().Be("Fail");
    
    [Fact]
    public void Failed_test_has_correct_Failure_Message() => _failedTest.Failure!.Message.Should().Be(
@"Assert.True() Failure
Expected: True
Actual:   False");
    
    [Fact]
    public void Failed_test_has_correct_Failure_StackTrace()
    {
        // Need to split the assertion in two, because the path of the file varies per build environment (e.g. local or github)
        _failedTest.Failure!.StackTrace.Should().StartWith(
            $@"   at xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_too_old.We_make_them_happy() in /");

        _failedTest.Failure!.StackTrace.Should().EndWith(
            $@"smink/ExampleTestProjects/xUnit.ExampleTests/xUnit.ExampleTests.Set1/TestSuites/Adding_a_new_customer/When_the_customer_is_too_old.cs:line 12
   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)");

    }
}