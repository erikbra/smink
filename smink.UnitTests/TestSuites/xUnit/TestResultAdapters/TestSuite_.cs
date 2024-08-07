using FluentAssertions;
using smink.Models.Report;
using smink.Models.XUnit;

// ReSharper disable InconsistentNaming

namespace smink.UnitTests.TestSuites.xUnit.TestResultAdapters;

[Collection(nameof(RunXUnitTestsFixture))]
public class TestSuite_
{
    private readonly Assemblies? _testResults;
    private readonly TestReport? _report;
    private readonly TestSuite? _testSuite;

    public TestSuite_(RunXUnitTestsFixture fixture)
    {
        _testResults = fixture.Assemblies;
        _report = fixture.TestReport;

        _testSuite = _report!.TestSuites.SingleOrDefault(s => "Adding a new customer".Equals(s.DisplayName));
    }
    
    [Fact]
    public void Is_not_null() => _testSuite.Should().NotBeNull();

    [Fact]
    public void Has_Timestamp() => _testSuite!.RunTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(15));

    // [Fact]
    // public void Has_Id() => _report!.Id.Should().Be(_testResults!.AssembliesList.First().Id.ToString());
    //
    [Fact]
    public void Has_Total() => _testSuite!.Total.Should().BeGreaterThan(1);

    [Fact]
    public void Has_Errors() => _testSuite!.Errors.Should().Be(0);

    [Fact]
    public void Has_Failed() => _testSuite!.Failed.Should().Be(1);

    [Fact]
    public void Has_Passed() => _testSuite!.Passed.Should().BeGreaterThanOrEqualTo(5);

    [Fact]
    public void Has_Skipped() => _testSuite!.Skipped.Should().Be(1);

    [Fact]
    public void Has_correct_Name() => _testSuite!.Name.Should().Be("Adding_a_new_customer");

    [Fact]
    public void Has_correct_Environment() => _testSuite!.Environment.Should().Be("Environment name specified on the command line");

    [Fact]
    public void Has_correct_DisplayName() => _testSuite!.DisplayName.Should().Be("Adding a new customer");
    
    [Fact]
    public void Has_correct_RootNamespace() => _testSuite!.RootNamespace.Should().Be("xUnit.ExampleTests.Set1");

    [Fact]
    public void Has_list_of_scenarios() => _testSuite!.TestScenarios.Should().HaveCount(2);

}