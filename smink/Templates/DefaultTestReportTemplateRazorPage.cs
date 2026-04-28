using System.Globalization;
using Microsoft.AspNetCore.Components;
using smink.Models.Report;

namespace smink.Templates;

public class DefaultTestReportTemplateRazorPage: ComponentBase
{
    protected const string PassTone = "pass";
    protected const string FailTone = "fail";
    protected const string ErrorTone = "error";
    protected const string SkipTone = "skip";
    protected const string UnknownTone = "unknown";

    [Parameter] public TestReport TestReport { get; set; } = new();

    protected static string FormatPercent(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    protected static string FormatSeconds(decimal seconds)
    {
        var totalMilliseconds = (double)seconds * 1000;
        if (totalMilliseconds < 1)
        {
            return "< 1 ms";
        }

        if (seconds < 1)
        {
            return $"{Math.Round(totalMilliseconds)} ms";
        }

        if (seconds < 60)
        {
            return $"{seconds.ToString("0.###", CultureInfo.InvariantCulture)} s";
        }

        var span = TimeSpan.FromSeconds((double)seconds);
        return span.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
    }

    protected static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 1)
        {
            return $"{Math.Round(duration.TotalMilliseconds)} ms";
        }

        if (duration.TotalMinutes < 1)
        {
            return $"{duration.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} s";
        }

        return duration.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
    }

    protected static string FormatTimestamp(DateTime? timestamp) =>
        timestamp.HasValue
            ? timestamp.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)
            : "Unknown";

    protected static string GetResultLabel(TestRun test) => test.Result switch
    {
        "Pass" => "Passed",
        "Skip" => "Skipped",
        "Fail" when test.Failure is not null => "Errored",
        "Fail" => "Failed",
        { Length: > 0 } value => value,
        _ => "Unknown"
    };

    protected static string GetResultTone(TestRun test) => test.Result switch
    {
        "Pass" => PassTone,
        "Skip" => SkipTone,
        "Fail" when test.Failure is not null => ErrorTone,
        "Fail" => FailTone,
        _ => UnknownTone
    };

    protected static string GetSuiteTone(TestSuite suite)
    {
        if (suite.Errors > 0)
        {
            return ErrorTone;
        }

        if (suite.Failed > 0)
        {
            return FailTone;
        }

        if (suite.Passed > 0 && suite.Passed == suite.Total)
        {
            return PassTone;
        }

        if (suite.Skipped > 0)
        {
            return SkipTone;
        }

        return UnknownTone;
    }

    protected static string GetScenarioTone(TestScenario scenario)
    {
        if (scenario.Errors > 0)
        {
            return ErrorTone;
        }

        if (scenario.Failed > 0)
        {
            return FailTone;
        }

        if (scenario.Passed > 0 && scenario.Passed == scenario.Total)
        {
            return PassTone;
        }

        if (scenario.Skipped > 0)
        {
            return SkipTone;
        }

        return UnknownTone;
    }

    protected static string ShortStatusLabel(string tone) => tone switch
    {
        PassTone => "Healthy",
        FailTone => "Failed",
        ErrorTone => "Errored",
        SkipTone => "Skipped",
        _ => "Unknown"
    };

    protected static string? Shorten(string? message) => message switch{
        { Length: > 2000} => message[..1999] + "...",
        _ => message 
    };
}