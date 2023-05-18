using FluentAssertions;
using smink.Models.Report;
using smink.TestResultAdapters;
using smink.TestResultReaders;

// ReSharper disable InconsistentNaming

namespace smink.UnitTests.TestSuites.TestResultAdapters_xUnit;

public class TestScenario_: IClassFixture<RunUnitTestsFixture>
{
    private readonly TestScenario _withSkipped;
    private readonly TestScenario _withFailed;

    public TestScenario_(RunUnitTestsFixture fixture)
    {
        var xunit = new XUnitResultsReader();
        var testReport = new XUnitResultsAdapter();

        var load = xunit.Load(fixture.TestResultsFiles.First());
        load.Wait();
        var testResults = load.Result;
        var report = testReport.Read(testResults);

        var testSuite = report!.TestSuites.Skip(1).FirstOrDefault();
        IEnumerable<TestScenario> scenarios = testSuite!.TestScenarios.ToList();

        _withSkipped = scenarios.First();
        _withFailed = scenarios.Skip(1).First();
    }
    
    [Fact]
    public void Has_Total() => _withSkipped!.Total.Should().BeGreaterThan(1);

    [Fact]
    public void Has_Errors() => _withFailed!.Errors.Should().Be(0);

    [Fact]
    public void Has_Failed() => _withFailed!.Failed.Should().Be(1);

    [Fact]
    public void Has_Passed() => _withSkipped.Passed.Should().Be(3);

    [Fact]
    public void Has_Skipped() => _withSkipped.Skipped.Should().Be(1);

    [Fact]
    public void Has_correct_Name() => _withSkipped!.Name.Should().Be("Test collection for xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed");

    [Fact]
    public void Has_correct_DisplayName() => _withSkipped!.DisplayName.Should().Be("When the customer is allowed");

}