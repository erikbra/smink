using FluentAssertions;
using FluentAssertions.Execution;
using smink.Models.Report;
using smink.Models.XUnit;
using smink.TestResultAdapters;
using smink.TestResultReaders;

// ReSharper disable InconsistentNaming

namespace smink.UnitTests.TestSuites.NUnit.TestResultAdapters;

[Collection(nameof(RunNUnitTestsFixture))]
public class TestReport_
{
    private readonly IEnumerable<Models.NUnit.TestRun> _testResults;
    private readonly TestReport? _report;

    public TestReport_(RunNUnitTestsFixture fixture)
    {
        _testResults = fixture.TestRuns;
        _report = fixture.TestReport;
    }
    
    [Fact]
    public void Is_not_null() => _report.Should().NotBeNull();

    [Fact]
    public void Has_Timestamp() => _report!.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(15));

    [Fact]
    public void Has_Id() => _report!.Id.Should().Be(_testResults.First().Id);

    [Fact]
    public void Has_correct_number_of_TotalTests() => _report!.TotalTests.Should().Be(_testResults.Sum(r => r.Total));

    //[Fact]
    //public void Has_correct_number_of_TotalErrors() => throw new NotImplementedException(); //_report!.TotalErrors.Should().Be(_testResults!.TotalErrors);

    [Fact]
    public void Has_correct_number_of_TotalFailures() => _report!.TotalFailures.Should().Be(_testResults!.Sum(r => r.Failed));

    [Fact]
    public void Has_correct_number_of_TotalSuccessful() => _report!.TotalSuccessful.Should().Be(_testResults!.Sum(r => r.Passed));

    [Fact]
    public void Flattens_and_groups_the_testSuites_following_conventions()
    {
        using (new AssertionScope())
        {
            var testSuites = _report!.TestSuites.ToList();
            testSuites.Should().HaveCount(5);
            
            var names = testSuites.Select(suite => suite.Name).ToArray();

            names.Should().Contain("NUnit.ExampleTests.Set1");
            names.Should().Contain("Adding_a_new_customer");
            names.Should().Contain("Buying_a_product");
        }
    }
}