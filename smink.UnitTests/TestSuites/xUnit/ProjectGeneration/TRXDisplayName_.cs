using AwesomeAssertions;
using smink;

namespace smink.UnitTests.TestSuites.xUnit.ProjectGeneration;

[Collection(nameof(RunXUnit3TestsFixture))]
public class TRXDisplayName_
{
    private readonly RunXUnit3TestsFixture _fixture;

    public TRXDisplayName_(RunXUnit3TestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Trx_test_names_use_method_name_only()
    {
        var report = await new TestReportGenerator(null).GenerateReport(_fixture.TestResultsFiles);
        report.Should().NotBeNull();

        var allTests = report!
            .TestSuites
            .SelectMany(suite => suite.TestScenarios)
            .SelectMany(scenario => scenario.Tests)
            .ToArray();

        allTests.Should().Contain(test => test.DisplayName == "The customer is registered");
        allTests.Should().NotContain(test =>
            (test.DisplayName ?? string.Empty).Contains("xUnit, ExampleTests, Set1", StringComparison.Ordinal));
    }
}
