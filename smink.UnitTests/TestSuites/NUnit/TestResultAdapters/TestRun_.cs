using FluentAssertions;
using smink.Models.Report;

// ReSharper disable InconsistentNaming

namespace smink.UnitTests.TestSuites.NUnit.TestResultAdapters;

[Collection(nameof(RunNUnitTestsFixture))]
public class TestRun_
{
    private readonly TestRun _passedTest;
    private readonly TestRun _skippedTest;
    private readonly TestRun _failedTest;
    private readonly RunNUnitTestsFixture _fixture;

    public TestRun_(RunNUnitTestsFixture fixture)
    {
        var report = fixture.TestReport;

        _fixture = fixture;

        var testSuite = report!.TestSuites.FirstOrDefault(s => "Adding a new customer".Equals(s.DisplayName));
        IEnumerable<TestScenario> scenarios = testSuite!.TestScenarios.ToList();

        var withSkipped = scenarios.First();
        var withFailed = scenarios.Skip(1).First();

        _passedTest = withSkipped.Tests.First(t => t.Name!.EndsWith("We_validate_the_mobile_number_of_the_customer"));
        _skippedTest = withSkipped.Tests.First(t => t.Name!.EndsWith("We_send_them_around_the_world_as_a_thank_you"));
        _failedTest = withFailed.Tests.First(t => t.Name!.EndsWith("We_make_them_happy"));
    }

    [Fact]
    public void Has_correct_Name() => _passedTest.Name.Should().Be("NUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed.We_validate_the_mobile_number_of_the_customer");

    [Fact]
    public void Has_correct_DisplayName() => _passedTest.DisplayName.Should().Be("We validate the mobile number of the customer");
    
    [Fact]
    public void Has_correct_Type() => _passedTest.Type.Should().Be("NUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed");
    
    [Fact]
    public void Has_correct_Method() => _passedTest.Method.Should().Be("We_validate_the_mobile_number_of_the_customer");
    
    [Fact]
    public void Passed_test_has_correct_Result() => _passedTest.Result.Should().Be("Pass");
    
    [Fact]
    public void Skipped_test_has_correct_Result() => _skippedTest.Result.Should().Be("Skip");
    
    [Fact(Skip = "NUnit reporter doesn't use the 'Reason', only the Output")]
    public void Skipped_test_has_correct_Reason() => _skippedTest.Reason.Should().Be("This might be too expensive");
    
    [Fact()]
    public void Skipped_test_has_correct_Output() => _skippedTest.Output.Should().Be(@"This might be too expensive
");

    [Fact()]
    public void Failed_test_has_correct_Result() => _failedTest.Result.Should().Be("Fail");
    
    [Fact]
    public void Failed_test_has_correct_Failure_Message() => _failedTest.Failure!.Message.Should().Be(
@"We cannot make them happy when they are too old");
    
    [Fact]
    public void Failed_test_has_correct_Failure_StackTrace() => _failedTest.Failure!.StackTrace.Should().Be(
$@"   at NUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_too_old.We_make_them_happy() in {_fixture.ExampleTestProjectFolder}/NUnit.ExampleTests.Set1/TestSuites/Adding_a_new_customer/When_the_customer_is_too_old.cs:line 12

1)    at NUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_too_old.We_make_them_happy() in {_fixture.ExampleTestProjectFolder}/NUnit.ExampleTests.Set1/TestSuites/Adding_a_new_customer/When_the_customer_is_too_old.cs:line 12

");
    
    
}