using FluentAssertions;
using smink.Models.Report;
using smink.TestResultAdapters;
using smink.TestResultReaders;

// ReSharper disable InconsistentNaming

namespace smink.UnitTests.TestSuites.NUnit.TestResultAdapters;

[Collection(nameof(RunNUnitTestsFixture))]
public class TestScenario_
{
    private readonly TestScenario _withSkipped;
    private readonly TestScenario _withFailed;

    public TestScenario_(RunNUnitTestsFixture fixture)
    {
        var report = fixture.TestReport;

        var testSuite = report!.TestSuites.FirstOrDefault(s => "Adding a new customer".Equals(s.DisplayName));
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
    public void Has_correct_Name() => _withSkipped!.Name.Should().Be("NUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed");

    [Fact]
    public void Has_correct_DisplayName() => _withSkipped!.DisplayName.Should().Be("When the customer is allowed");

}