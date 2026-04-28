using System.Globalization;
using System.Text.Json;
using smink.Models.XUnit;

namespace smink.TestResultReaders;

// CTRF format: https://ctrf.io/
public class CTRFResultsReader
{
    public Task<Assemblies?> Load(IEnumerable<string> files) => Load(files.ToArray());

    public async Task<Assemblies?> Load(params string[] files)
    {
        var assemblies = new Assemblies();

        foreach (var file in files)
        {
            await using var stream = File.OpenRead(file);
            using var document = await JsonDocument.ParseAsync(stream);

            var assembly = Parse(document, Path.GetFileNameWithoutExtension(file));
            if (assembly is not null)
            {
                assemblies.AssembliesList.Add(assembly);
            }
        }

        return assemblies;
    }

    private static Assembly? Parse(JsonDocument document, string fileName)
    {
        var root = document.RootElement;

        var tests = GetTestsElement(root);
        if (tests is null || tests.Value.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var collectionMap = new Dictionary<string, Collection>();
        string? rootNamespace = null;

        foreach (var testElement in tests.Value.EnumerateArray())
        {
            var status = GetString(testElement, "status") ?? string.Empty;
            var suite = GetString(testElement, "suite") ?? GetString(testElement, "class") ?? "Default";
            var name = GetString(testElement, "name") ?? GetString(testElement, "title") ?? "Unknown";
            var durationInSeconds = GetDurationInSeconds(testElement);

            var normalizedSuite = suite.StartsWith("Test collection for ", StringComparison.Ordinal)
                ? suite
                : $"Test collection for {suite}";

            if (rootNamespace is null)
            {
                rootNamespace = ExtractRootNamespace(suite) ?? fileName;
            }

            if (!collectionMap.TryGetValue(normalizedSuite, out var collection))
            {
                collection = new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = normalizedSuite,
                    Tests = new List<Test>()
                };
                collectionMap[normalizedSuite] = collection;
            }

            var result = MapStatus(status);
            var test = new Test
            {
                Id = Guid.NewGuid(),
                Name = name,
                Type = "Unit",
                Method = name,
                Time = durationInSeconds,
                Result = result
            };

            if (result == Result.Fail)
            {
                test.Failure = new Failure
                {
                    ExceptionType = "Assertion",
                    Message = GetString(testElement, "message") ?? "Test failed",
                    StackTrace = GetString(testElement, "trace") ?? string.Empty
                };
                collection.Failed++;
            }
            else if (result == Result.Skip)
            {
                test.Reason = GetString(testElement, "message") ?? "Test skipped";
                collection.Skipped++;
            }
            else
            {
                collection.Passed++;
            }

            collection.Tests.Add(test);
        }

        var assembly = new Assembly
        {
            Id = Guid.NewGuid(),
            Name = $"{rootNamespace ?? fileName}.dll",
            Collections = collectionMap.Values.ToList()
        };

        foreach (var collection in assembly.Collections)
        {
            collection.NotRun = collection.Skipped;
            collection.Total = collection.Tests.Count;
            collection.Time = collection.Tests.Sum(t => t.Time);
        }

        assembly.Passed = assembly.Collections.Sum(c => c.Passed);
        assembly.Failed = assembly.Collections.Sum(c => c.Failed);
        assembly.Skipped = assembly.Collections.Sum(c => c.Skipped);
        assembly.NotRun = assembly.Skipped;
        assembly.Total = assembly.Collections.Sum(c => c.Total);

        return assembly;
    }

    private static JsonElement? GetTestsElement(JsonElement root)
    {
        if (root.TryGetProperty("results", out var results) &&
            results.ValueKind == JsonValueKind.Object &&
            results.TryGetProperty("tests", out var tests))
        {
            return tests;
        }

        if (root.TryGetProperty("tests", out var topLevelTests))
        {
            return topLevelTests;
        }

        return null;
    }

    private static Result MapStatus(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "pass" or "passed" => Result.Pass,
            "skip" or "skipped" or "pending" or "todo" => Result.Skip,
            _ => Result.Fail
        };
    }

    private static decimal GetDurationInSeconds(JsonElement testElement)
    {
        if (!testElement.TryGetProperty("duration", out var duration))
        {
            return 0;
        }

        return duration.ValueKind switch
        {
            JsonValueKind.Number when duration.TryGetDecimal(out var asDecimal) => asDecimal / 1000m,
            JsonValueKind.String when decimal.TryParse(duration.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var asStringDecimal)
                => asStringDecimal / 1000m,
            _ => 0
        };
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? ExtractRootNamespace(string suite)
    {
        var source = suite.StartsWith("Test collection for ", StringComparison.Ordinal)
            ? suite["Test collection for ".Length..]
            : suite;

        var parts = source.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            return null;
        }

        return string.Join('.', parts.Take(3));
    }
}
