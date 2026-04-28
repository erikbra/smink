using System.Globalization;
using System.Xml.Linq;
using smink.Models.XUnit;
using static smink.TestResultReaders.XmlUtils;

namespace smink.TestResultReaders;

// Microsoft Visual Studio TeamTest TRX format
// https://learn.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2012/dd409404(v=vs.110)

public class TRXResultsReader
{
    public Task<Assemblies?> Load(IEnumerable<string> files) => Load(files.ToArray());

    public async Task<Assemblies?> Load(params string[] files)
    {
        var ct = new CancellationToken();
        var assemblies = new Assemblies();

        foreach (var file in files)
        {
            await using var stream = File.OpenRead(file);
            var doc = await XDocument.LoadAsync(stream, LoadOptions.PreserveWhitespace, ct);
            var theseResults = Parse(doc, Path.GetFileNameWithoutExtension(file));
            if (theseResults is { })
            {
                assemblies.AssembliesList.AddRange(theseResults.AssembliesList);
            }
        }

        return assemblies;
    }

    private Assemblies? Parse(XDocument doc, string fileName)
    {
        var root = doc.Root;
        if (root?.Name.LocalName != "TestRun")
            return null;

        var ns = root.Name.NamespaceName;
        var xns = XNamespace.Get(ns);

        var assemblyName = $"{fileName}.dll";
        var assembly = new Assembly
        {
            Name = assemblyName,
            Id = Guid.NewGuid(),
            Collections = new List<Collection>()
        };

        // Parse TestDefinitions to get className for each test (maps to xUnit collection/scenario)
        var testDefinitions = new Dictionary<string, (string className, string method)>();
        var testDefsElement = root.Element(xns + "TestDefinitions");
           string? actualRootNamespace = null;
           if (testDefsElement is not null)
        {
            foreach (var unitTest in testDefsElement.Elements(xns + "UnitTest"))
            {
                var testId = unitTest.Attribute("id")?.Value;
                var testMethod = unitTest.Element(xns + "TestMethod");
                if (testId is not null && testMethod is not null)
                {
                    var className = testMethod.Attribute("className")?.Value ?? string.Empty;
                    var methodName = testMethod.Attribute("name")?.Value ?? string.Empty;
                    testDefinitions[testId] = (className, methodName);
                       // Extract the root namespace from the first className
                       if (actualRootNamespace is null && className.Contains("."))
                       {
                           var parts = className.Split('.');
                           if (parts.Length >= 3)
                           {
                               actualRootNamespace = $"{parts[0]}.{parts[1]}.{parts[2]}";
                           }
                       }
                }
            }
        }
           // Update assembly name to use actual root namespace
           if (actualRootNamespace is not null)
           {
               assemblyName = $"{actualRootNamespace}.dll";
               assembly.Name = assemblyName;
           }

        // Parse results
        var resultsElement = root.Element(xns + "Results");
        if (resultsElement is null)
            return new Assemblies { AssembliesList = [assembly] };

        var testResults = resultsElement.Elements(xns + "UnitTestResult").ToList();

        if (testResults.Count == 0)
        {
            var assemblies = new Assemblies { AssembliesList = [assembly] };
            return assemblies;
        }

        // Group tests by className (which represents the xUnit class/scenario)
        var collectionMap = new Dictionary<string, Collection>();

        foreach (var result in testResults)
        {
            var testName = result.Attribute("testName")?.Value ?? "Unknown";
            var testId = result.Attribute("testId")?.Value;
            var outcome = result.Attribute("outcome")?.Value ?? "Unknown";
            var duration = result.Attribute("duration")?.Value ?? "00:00:00.0000000";

            // Parse duration
            if (!TimeSpan.TryParse(duration, CultureInfo.InvariantCulture, out var parsedDuration))
            {
                parsedDuration = TimeSpan.Zero;
            }

            var timeDecimal = (decimal)parsedDuration.TotalSeconds;

            // Get className from TestDefinitions (this is the key to matching xUnit format!)
            var className = string.Empty;
            var methodName = ExtractMethodName(testName);
            if (testId is not null && testDefinitions.TryGetValue(testId, out var defInfo))
            {
                className = defInfo.className;
                if (!string.IsNullOrWhiteSpace(defInfo.method))
                {
                    methodName = defInfo.method;
                }
            }

            // Use className as collection name prefix (like xUnit format)
            // Collection name should be "Test collection for {className}"
            var collectionName = $"Test collection for {className}";

            // Create collection if not exists
            if (!collectionMap.TryGetValue(collectionName, out var collection))
            {
                collection = new Collection
                {
                    Name = collectionName,
                    Id = Guid.NewGuid(),
                    Tests = new List<Test>()
                };
                collectionMap[collectionName] = collection;
            }

            var test = new Test
            {
                Id = Guid.NewGuid(),
                Name = methodName,
                Type = "Unit",
                Method = methodName,
                Time = timeDecimal,
                Result = MapOutcome(outcome)
            };

            // Extract failure/skip info from Output
            var outputElement = result.Element(xns + "Output");
            if (outputElement is not null)
            {
                var errorInfoElement = outputElement.Element(xns + "ErrorInfo");
                if (errorInfoElement is not null)
                {
                    var message = errorInfoElement.Element(xns + "Message")?.Value;
                    var stackTrace = errorInfoElement.Element(xns + "StackTrace")?.Value;

                    if (outcome == "Failed")
                    {
                        test.Failure = new Failure
                        {
                            ExceptionType = "Assertion",
                            Message = message ?? "Test failed",
                            StackTrace = stackTrace ?? string.Empty
                        };
                        collection.Failed++;
                    }
                    else if (outcome == "NotExecuted")
                    {
                        test.Reason = message ?? "Test skipped";
                        collection.Skipped++;
                    }
                }
            }

            if (outcome == "Passed")
                collection.Passed++;

            collection.Tests.Add(test);
        }

        // Finalize collections
        foreach (var collection in collectionMap.Values)
        {
            collection.NotRun = collection.Skipped;
            collection.Total = collection.Tests.Count;
            collection.Time = collection.Tests.Sum(t => t.Time);
            assembly.Collections.Add(collection);
        }

        // Update assembly totals
        assembly.Passed = collectionMap.Values.Sum(c => c.Passed);
        assembly.Failed = collectionMap.Values.Sum(c => c.Failed);
        assembly.Skipped = collectionMap.Values.Sum(c => c.Skipped);
        assembly.NotRun = assembly.Skipped;
        assembly.Total = collectionMap.Values.Sum(c => c.Total);

        return new Assemblies { AssembliesList = [assembly] };
    }

    private static string ExtractMethodName(string testName)
    {
        if (string.IsNullOrWhiteSpace(testName))
        {
            return "Unknown";
        }

        var commaIndex = testName.LastIndexOf(',');
        var candidate = commaIndex >= 0 ? testName[(commaIndex + 1)..] : testName;
        return candidate.Trim().Replace(" ", "_");
    }

    private static Result? MapOutcome(string outcome)
    {
        return outcome switch
        {
            "Passed" => Result.Pass,
            "Failed" => Result.Fail,
            "NotExecuted" => Result.Skip,
            _ => null
        };
    }
}
