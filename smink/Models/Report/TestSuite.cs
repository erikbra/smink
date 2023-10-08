namespace smink.Models.Report;

public class TestSuite
{
    public IEnumerable<TestScenario> TestScenarios { get; set; } = new List<TestScenario>();
    
    public string? Id { get; set;  }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string SanitizedName => Common.Sanitize(Name ?? Id!.ToString());
    
    public string? ConfigFile { get; set; }
    public string? Environment { get; set; }
    
    public decimal Time { get; set; }
    public DateTime? RunTime { get; set; }
    
    public string? TargetFramework { get; set; }
    public string? TestFramework { get; set; }
    
    public int Passed => TestScenarios.Sum(c => c.Passed);
    public int Skipped => TestScenarios.Sum(c => c.Skipped);
    public int Errors => TestScenarios.Sum(c => c.Errors);
    public int Failed => TestScenarios.Sum(c => c.Failed);
    public int NotRun => TestScenarios.Sum(c => c.NotRun);
    public int Total => TestScenarios.Sum(c => c.Total);
    
    public decimal TotalTime => TestScenarios.Sum(c => c.Time);
    public int TotalMinutes => TestScenarios.Sum(c => ((int) Math.Floor(c.Time)) / 60);
    public int TotalSeconds => TestScenarios.Sum(c => ((int) Math.Round(c.Time)) % 60);

    public decimal SuccessRate =>
        Total switch
        {
            > 0 => (100.0M * Passed) / Total,
            _ => 0
        };

    public string? RootNamespace { get; set; }
}