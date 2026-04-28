using smink.Models.Report;
using smink.TestResultAdapters;
using smink.TestResultReaders;
using System.Text.Json;

namespace smink;

public class TestReportGenerator
{
    private readonly string? _title;

    public TestReportGenerator(string? title)
    {
        _title = title;
    }

    public async Task<TestReport?> GenerateReport(IEnumerable<string> files)
    {
        var filesArray = files.ToArray();
        var typedFiles = filesArray
            .Select(file => new { File = file, Type = Identify(file) })
            .ToArray();

        var unknown = typedFiles.FirstOrDefault(f => f.Type is null);
        if (unknown is not null)
        {
            throw new UnknownReportType($"{unknown.File} (unknown format)");
        }

        var reports = new List<TestReport>();

        foreach (var group in typedFiles.GroupBy(f => f.Type!))
        {
            var report = group.Key switch
            {
                "xUnit" => await GenerateXUnitReport(group.Select(f => f.File)),
                "NUnit" => await GenerateNUnitReport(group.Select(f => f.File)),
                "TRX" => await GenerateTRXReport(group.Select(f => f.File)),
                "CTRF" => await GenerateCTRFReport(group.Select(f => f.File)),
                _ => throw new UnknownReportType(group.Key)
            };

            if (report is not null)
            {
                reports.Add(report);
            }
        }

        var mergedReport = MergeReports(reports);
        SetTitle(mergedReport);
        return mergedReport;

    }

    private async Task<TestReport?> GenerateXUnitReport(IEnumerable<string> files)
    {
        var xunit = new XUnitResultsReader();
        var testResults = await xunit.Load(files);
        var testReport = new XUnitResultsAdapter().Read(testResults);
        SetTitle(testReport);
        return testReport;
    }

    private async Task<TestReport?> GenerateNUnitReport(IEnumerable<string> files)
    {
        var nUnit = new NUnitResultsReader();
        var testResults = await nUnit.Load(files);
        var testReport = new NUnitResultsAdapter().Read(testResults);
        SetTitle(testReport);
        return testReport;
    }

    private async Task<TestReport?> GenerateTRXReport(IEnumerable<string> files)
    {
        var trx = new TRXResultsReader();
        var testResults = await trx.Load(files);
        var testReport = new XUnitResultsAdapter().Read(testResults);
        SetTitle(testReport);
        return testReport;
    }

    private async Task<TestReport?> GenerateCTRFReport(IEnumerable<string> files)
    {
        var ctrf = new CTRFResultsReader();
        var testResults = await ctrf.Load(files);
        var testReport = new XUnitResultsAdapter().Read(testResults);
        return testReport;
    }

    private static string? Identify(string? file)
    {
        if (file is null)
        {
            return null;
        }

        using var s = File.OpenText(file);
        var l = s.ReadLine();
        while (l is { } && (l.Length == 0 || l.StartsWith("<?xml") || string.IsNullOrWhiteSpace(l)))
        {
            l = s.ReadLine();
        }

        var trimmed = l?.TrimStart();
        if (trimmed?.StartsWith("{") == true)
        {
            return IsCtrf(file) ? "CTRF" : null;
        }

        return trimmed switch
        {
            { } when trimmed.StartsWith("<assemblies") => "xUnit",
            { } when trimmed.StartsWith("<test-run") => "NUnit",
            { } when trimmed.StartsWith("<TestRun") => "TRX",
            _ => null
        };
    }

    private static bool IsCtrf(string file)
    {
        try
        {
            using var stream = File.OpenRead(file);
            using var document = JsonDocument.Parse(stream);

            if (document.RootElement.TryGetProperty("reportFormat", out var reportFormat) &&
                reportFormat.ValueKind == JsonValueKind.String &&
                string.Equals(reportFormat.GetString(), "CTRF", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (document.RootElement.TryGetProperty("results", out var results) &&
                results.ValueKind == JsonValueKind.Object &&
                results.TryGetProperty("tests", out var tests) &&
                tests.ValueKind == JsonValueKind.Array)
            {
                return true;
            }

            if (document.RootElement.TryGetProperty("tests", out var topLevelTests) &&
                topLevelTests.ValueKind == JsonValueKind.Array)
            {
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    private static TestReport? MergeReports(IEnumerable<TestReport> reports)
    {
        var reportList = reports.ToList();
        if (!reportList.Any())
        {
            return null;
        }

        var first = reportList.First();
        var mergedSuites = reportList.SelectMany(r => r.TestSuites).ToArray();

        return new TestReport
        {
            Id = first.Id,
            Title = first.Title,
            Computer = first.Computer,
            Timestamp = reportList.Where(r => r.Timestamp is not null).Select(r => r.Timestamp).Min(),
            User = first.User,
            TestSuites = mergedSuites,
            TotalTests = mergedSuites.Sum(s => s.Total),
            TotalErrors = mergedSuites.Sum(s => s.Errors),
            TotalFailures = mergedSuites.Sum(s => s.Failed),
            TotalSuccessful = mergedSuites.Sum(s => s.Passed)
        };
    }
    
    private void SetTitle(TestReport? testReport)
    {
        if (testReport is { } && _title is { Length: > 0 })
        {
            testReport.Title = _title;
        }
    }

}

public class UnknownReportType : Exception
{
    public UnknownReportType(string? typeOfReport): base("Unknown report type: " + typeOfReport)
    {}
}