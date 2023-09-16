using System.Collections;
using smink.Models.NUnit;
using smink.Models.Report;
using smink.Models.XUnit;
using TestRun = smink.Models.Report.TestRun;
using TestSuite = smink.Models.NUnit.TestSuite;

namespace smink.TestResultAdapters;

//https://docs.nunit.org/articles/nunit/technical-notes/usage/Test-Result-XML-Format.html

public class NUnitResultsAdapter
{
    public TestReport? Read(IEnumerable<Models.NUnit.TestRun> testRuns)
    {
        // Just pick some info from the first test run to put on the test report
        testRuns = testRuns.ToArray();
        var first = testRuns.FirstOrDefault();

        if (first is null)
        {
            return null;
        }
        
        return testRuns switch
        {
            { } => new TestReport()
            {
                Id = first.Id,
                Timestamp = first.StartTime?.ToUniversalTime(),
                //Computer = string.Empty,
                //User = string.Empty,
                TestSuites = testRuns.SelectMany(r => r.TestSuites.SelectMany(MapSuite)),
                TotalTests = testRuns.Sum(a => a.Total) ?? 0,
                //TotalErrors = testRuns.Sum(a => a.),
                TotalFailures = testRuns.Sum(a => a.Failed) ?? 0,
                TotalSuccessful = testRuns.Sum(a => a.Passed) ?? 0
            },
            _ => null
        };
    }

    private IEnumerable<smink.Models.Report.TestSuite> MapSuite(Models.NUnit.TestSuite suite)
    {
        var rootNamespace = GetName(suite);
        
        // Flatten out the hierarchy of test suites, we don't need the deep hierarchy
        var suites = suite.TestSuites.SelectMany(Flatten).ToArray();
        foreach (var s in suites)
        {
            s.TestSuites = FindFixtures(s.TestSuites);
        }

        return suites.Select(s => new Models.Report.TestSuite()
        {
            Name = GetName(s),
            DisplayName = Common.DisplayName(s.Name),
            Time = s.Duration!.Value,
            RunTime = s.StartTime?.ToUniversalTime(),
            RootNamespace = rootNamespace,

            TestScenarios = s.TestSuites.Select(MapToScenario),
        });
    }

    private IEnumerable<Models.NUnit.TestSuite> Flatten(Models.NUnit.TestSuite suite)
    {
        return suite.TestSuites.Any(s => "TestFixture".Equals(s.Type, StringComparison.InvariantCultureIgnoreCase))
            ? new[] { suite }
            : suite.TestSuites.SelectMany(Flatten);
    }

    private TestScenario MapToScenario(Models.NUnit.TestSuite suite)
    {
        return new TestScenario()
        {
            Id = Guid.NewGuid(),
            Name = GetName(suite),
            DisplayName = Common.DisplayName(suite.Name),

            Passed = suite.Passed!.Value,
            Failed = suite.Failed!.Value,
            Skipped = suite.Skipped!.Value,
            NotRun = suite.Inconclusive!.Value,
            Total = suite.Total!.Value,

            Time = suite.Duration!.Value,

            Tests = suite.TestCases.Select(fixture => MapToTest(suite, fixture))
        };
    }

    private TestRun MapToTest(TestSuite testSuite, TestCase testCase)
    {
        var displayName = Common.DisplayName(testCase.Name);
        
        return new Models.Report.TestRun()
        {
            Id = Guid.NewGuid(),
            Name = testCase.FullName,
            DisplayName = displayName,
            Type = testSuite.FullName,
            Method = testCase.MethodName,
            Result = GetResult(testCase.Result),
            Time = testCase.Duration!.Value,
            
            Output = testCase.Output,
            Failure = GetFailure(testCase)
            
        };
    }

    private static TestFailure? GetFailure(TestCase s)
    {
        var failure = s.Failures.SingleOrDefault();
        
        return failure switch
        {
            { } => new TestFailure
            {
                Message = failure.Message,
                StackTrace = failure.StackTrace
            },
            _ => null
        };
    }

    private IEnumerable<Models.NUnit.TestSuite> FindFixtures(IEnumerable<Models.NUnit.TestSuite> suites)
    {
        return suites.SelectMany(suite => suite.Type switch
        {
            "TestFixture" => new []{ suite },
            _ => FindFixtures(suite.TestSuites)
        });
    }

    private static string? GetName(TestSuite suite) =>
        suite.Type switch
        {
            "Assembly" => Path.GetFileNameWithoutExtension(suite.FullName) ?? string.Empty,
            "TestSuite" => suite.FullName switch
                {
                    null => string.Empty,
                    { } when suite.FullName.Contains(".TestSuites.") => suite.Name,
                    { } => suite.FullName
                },
            "TestFixture" => suite.FullName, 
            _ => suite.Name
        };

    private static string? GetResult(string? result)
        => result switch
        {
            "Passed" => "Pass",
            "Failed" => "Fail",
            "Skipped" => "Skip",
            _ => result
        };
}