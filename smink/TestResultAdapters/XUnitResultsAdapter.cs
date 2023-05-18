using System.Globalization;
using smink.Models.Report;
using smink.Models.XUnit;
using Test = smink.Models.XUnit.Test;

namespace smink.TestResultAdapters;

// https://xunit.net/docs/format-xml-v2

public class XUnitResultsAdapter
{
    public TestReport? Read(Assemblies? assemblies)
    {
        return assemblies switch
        {
            { } => new TestReport()
                {
                    Id = assemblies.Id,
                    Timestamp = TryParse(assemblies.Timestamp),
                    Computer = assemblies.Computer,
                    User = assemblies.User,
                    TestSuites = assemblies.AssembliesList.Select(GetTestSuites).SelectMany(suite => suite)
                },
            _ => null
        };
    }

    private static DateTime? TryParse(string? timestamp) =>
        timestamp switch
        {
            { Length: > 0 } when DateTime.TryParse(timestamp, DateTimeFormatInfo.InvariantInfo, out var time) => time,
            _ => null
        };
    
    private static IEnumerable<TestSuite> GetTestSuites(Assembly arg)
    {
        var fullAssemblyName = arg.Name;
        var rootNamespace = Path.GetFileNameWithoutExtension(fullAssemblyName) ?? string.Empty;

        var testSuites =
            arg.Collections
                .Select(collection => GetTestSuite(arg, rootNamespace, collection))
                .DistinctBy(suite => suite.Name)
                .ToList();

        foreach (var suite in testSuites)
        {
            suite.TestScenarios = arg.Collections
                .Where(collection => IsInSuite(suite, collection))
                .OrderBy(c => c.Name)
                .Select(collection => GetTestScenario(suite, collection))
                .ToList()
                ;

            // Avoid counting time spent multiple times on assemblies split in multiple test suites
            if (!suite.Name!.Equals(suite.RootNamespace))
            {
                suite.Time = 0;
            }
            
        }

        return testSuites;
    }

    private static bool IsInSuite(TestSuite suite, Collection collection)
    {
        var suiteName = GetSuiteName(collection, suite.RootNamespace ?? string.Empty);
        return suiteName.Equals(suite.Name);
    }

    private static string GetSuiteName(Collection collection, string rootNamespace)
    {
        var testSuiteName = rootNamespace;
        var thisCollectionName = RemovePrefixes(collection.Name, rootNamespace);
        if (thisCollectionName is {} && thisCollectionName.StartsWith("TestSuites"))
        {
            var start = thisCollectionName.IndexOf(".", StringComparison.InvariantCulture) + 1;
            var end = thisCollectionName.IndexOf(".", start, StringComparison.InvariantCulture);
            testSuiteName = thisCollectionName[start..end];
        }

        return testSuiteName;
    }

    private static TestSuite GetTestSuite(Assembly arg, string rootNamespace, Collection collection)
    {
        var testSuiteName = rootNamespace;

        var thisCollectionName = RemovePrefixes(collection.Name, rootNamespace);
        if (thisCollectionName is {} && thisCollectionName.StartsWith("TestSuites"))
        {
            var start = thisCollectionName.IndexOf(".", StringComparison.InvariantCulture) + 1;
            var end = thisCollectionName.IndexOf(".", start, StringComparison.InvariantCulture);

            //rootNamespace += "." + thisCollectionName[..end];
            testSuiteName = thisCollectionName[start..end];
        }
        
        var displayName= Common.DisplayName(testSuiteName);

        DateOnly.TryParse(arg.RunDate, out DateOnly runDate);
        TimeOnly.TryParse(arg.RunTime, out TimeOnly runTime);

        DateTime runDateAndTime = runDate.ToDateTime(runTime);
        
        return new  TestSuite
        {
            Id = arg.Id,
            Name = testSuiteName,
            DisplayName = displayName,
            RootNamespace = rootNamespace,
            Environment = arg.Environment,
            Time = arg.Time,
            RunTime = runDateAndTime
        };
    }


    private static TestScenario GetTestScenario(TestSuite suite, Collection collection)
    {
        var parentNamespace = suite.RootNamespace;
        
        var scenarioName = RemovePrefixes(collection.Name, parentNamespace);

        scenarioName = RemovePrefix(scenarioName, $"TestSuites.{suite.Name}.");
        scenarioName = RemovePrefix(scenarioName, $"Test collection for ");
       
        var displayName = Common.DisplayName(scenarioName);
        
        return new TestScenario
        {
            Id = collection.Id,
            Name = collection.Name,
            DisplayName = displayName,

            Passed = collection.Passed,
            Failed = collection.Failed,
            Skipped = collection.Skipped,
            Errors = collection.Errors,
            NotRun = collection.NotRun,
            Total = collection.Total,

            Time = collection.Time,

            Tests = collection.Tests.Select(test => GetTest(scenarioName, test))
        };
    }

    private static TestRun GetTest(string? parentNamespace, Test arg)
    {
        var displayName = Common.DisplayName(arg.Method);
        var name = arg.Type + "." + arg.Method;
        
        return new TestRun
        {
            Id = arg.Id,
            Name = name,
            DisplayName = displayName,
            Type = arg.Type,
            Method = arg.Method,
            Result = arg.Result.ToString(),
            Time = arg.Time,
            
            Reason = arg.Reason,
            Output = arg.Output,

            Failure = arg.Failure switch
            {
                { } failure => new TestFailure()
                {
                    Message = failure.Message,
                    ExceptionType = failure.ExceptionType,
                    StackTrace = failure.StackTrace
                },
                _ => null
            },
        };
    }
    
    private static string? RemovePrefixes(string? name, string? parentNamespace)
    {
        const string prefixToRemove = "Test collection for ";
        string toRemove = prefixToRemove + parentNamespace + ".";

        return name?.Replace(toRemove, string.Empty);
    }

    private static string? RemovePrefix(string? s, string? prefix)
    {
        if (s is null || prefix is null)
        {
            return s;
        }
        
        var toReturn = s;
        if (toReturn.StartsWith(prefix))
        {
            toReturn = s[(prefix.Length)..];
        }

        return toReturn;
    }
}