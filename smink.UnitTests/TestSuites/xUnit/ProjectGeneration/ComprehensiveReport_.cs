using AwesomeAssertions;
using smink;

namespace smink.UnitTests.TestSuites.xUnit.ProjectGeneration;

[CollectionDefinition("RunMixedFormatsFixture")]
public class RunMixedFormatsFixtureCollection :
    ICollectionFixture<RunNUnitTestsFixture>,
    ICollectionFixture<RunXUnitTestsFixture>
{ }

[Collection("RunMixedFormatsFixture")]
public class ComprehensiveReport_
{
    private readonly RunNUnitTestsFixture _nunitFixture;
    private readonly RunXUnitTestsFixture _xunitFixture;

    public ComprehensiveReport_(RunNUnitTestsFixture nunitFixture, RunXUnitTestsFixture xunitFixture)
    {
        _nunitFixture = nunitFixture;
        _xunitFixture = xunitFixture;
    }

    [Fact]
    public async Task Mixed_input_formats_are_merged_in_one_report()
    {
      var inputFiles = _nunitFixture.TestResultsFiles
        .Concat(_xunitFixture.TestResultsFiles)
        .ToArray();

      var report = await new TestReportGenerator(null).GenerateReport(inputFiles);

        report.Should().NotBeNull();
      report!.TestSuites.Should().Contain(suite =>
        (suite.RootNamespace ?? string.Empty).StartsWith("NUnit.ExampleTests.", StringComparison.Ordinal));
      report.TestSuites.Should().Contain(suite =>
        (suite.RootNamespace ?? string.Empty).StartsWith("xUnit.ExampleTests.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CTRF_input_is_supported()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"smink-ctrf-{Guid.NewGuid():N}.json");

        await File.WriteAllTextAsync(tempFile, """
        {
          "reportFormat": "CTRF",
          "results": {
            "tool": { "name": "xUnit.net v3" },
            "tests": [
              {
                "name": "The_customer_is_registered",
                "suite": "xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed",
                "status": "passed",
                "duration": 45
              },
              {
                "name": "We_make_them_happy",
                "suite": "xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_too_old",
                "status": "failed",
                "duration": 12,
                "message": "Expected true but found false",
                "trace": "at Example.Test()"
              }
            ]
          }
        }
        """);

        try
        {
            var report = await new TestReportGenerator(null).GenerateReport(new[] { tempFile });

            report.Should().NotBeNull();
            report!.TotalTests.Should().Be(2);
            report.TotalFailures.Should().Be(1);
            report.TestSuites.Should().Contain(suite => suite.Name == "Adding_a_new_customer");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task CTRF_keeps_multiple_tests_in_same_scenario()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"smink-ctrf-multi-{Guid.NewGuid():N}.json");

        await File.WriteAllTextAsync(tempFile, """
        {
          "reportFormat": "CTRF",
          "results": {
            "tests": [
              {
                "name": "The_customer_is_registered",
                "suite": "xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed",
                "status": "passed",
                "duration": 45
              },
              {
                "name": "We_send_out_a_welcome_email",
                "suite": "xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed",
                "status": "passed",
                "duration": 31
              },
              {
                "name": "We_validate_the_mobile_number_of_the_customer",
                "suite": "xUnit.ExampleTests.Set1.TestSuites.Adding_a_new_customer.When_the_customer_is_allowed",
                "status": "passed",
                "duration": 28
              }
            ]
          }
        }
        """);

        try
        {
            var report = await new TestReportGenerator(null).GenerateReport(new[] { tempFile });

            report.Should().NotBeNull();
            var scenario = report!
                .TestSuites
                .SelectMany(suite => suite.TestScenarios)
                .Single(s => s.DisplayName == "When the customer is allowed");

            scenario.Total.Should().Be(3);
            scenario.Tests.Should().HaveCount(3);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

}
