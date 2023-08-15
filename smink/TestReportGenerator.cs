using smink.Models.Report;
using smink.TestResultAdapters;
using smink.TestResultReaders;

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
        // For now, only support one test result type per report - improve later
        files = files.ToArray();
        var typeOfReport = Identify(files.FirstOrDefault());

        return typeOfReport switch
        {
            "xUnit" => await GenerateXUnitReport(files),
            "NUnit" => await GenerateNUnitReport(files),
            _ => throw new UnknownReportType(typeOfReport)
        };

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

    private static string? Identify(string? file)
    {
        if (file is null)
        {
            return null;
        }

        using var s = File.OpenText(file);
        var l = s.ReadLine();
        return l switch
        {
            { } when l.StartsWith("<assemblies") => "xUnit",
            { } when l.StartsWith("<test-run") => "NUnit",
            _ => null
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