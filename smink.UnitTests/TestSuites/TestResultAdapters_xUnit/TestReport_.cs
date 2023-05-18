using FluentAssertions;
using FluentAssertions.Execution;
using smink.Models.Report;
using smink.Models.XUnit;
using smink.TestResultAdapters;
using smink.TestResultReaders;

// ReSharper disable InconsistentNaming

namespace smink.UnitTests.TestSuites.TestResultAdapters_xUnit;

public class TestReport_: IClassFixture<RunUnitTestsFixture>
{
    private readonly Assemblies? _testResults;
    private readonly TestReport? _report;

    public TestReport_(RunUnitTestsFixture fixture)
    {
        var xunit = new XUnitResultsReader();
        var testReport = new XUnitResultsAdapter();

        var load = xunit.Load(fixture.TestResultsFiles.First());
        load.Wait();
        _testResults = load.Result;
        _report = testReport.Read(_testResults);
    }
    
    [Fact]
    public void Is_not_null() => _report.Should().NotBeNull();

    [Fact]
    public void Has_Timestamp() => _report!.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(15));

    [Fact]
    public void Has_Id() => _report!.Id.Should().Be(_testResults!.Id);

    [Fact]
    public void Has_correct_number_of_TotalTests() => _report!.TotalTests.Should().Be(_testResults!.TotalTests);

    [Fact]
    public void Has_correct_number_of_TotalErrors() => _report!.TotalErrors.Should().Be(_testResults!.TotalErrors);

    [Fact]
    public void Has_correct_number_of_TotalFailures() => _report!.TotalFailures.Should().Be(_testResults!.TotalFailures);

    [Fact]
    public void Has_correct_number_of_TotalSuccessful() => _report!.TotalSuccessful.Should().Be(_testResults!.TotalSuccessful);

    [Fact]
    public void Groups_the_TestSuites_using_folder_convention()
    {
        using (new AssertionScope())
        {
            var testSuites = _report!.TestSuites.ToList();
            testSuites.Should().HaveCount(3);
            testSuites.First().Name.Should().Be("xUnit.ExampleTests.Set1");
            testSuites.Skip(1).First().Name.Should().Be("Adding_a_new_customer");
            testSuites.Skip(2).First().Name.Should().Be("Buying_a_product");
        }
    }
}