namespace smink.Models.Report;

public class TestReport
{
    public IEnumerable<TestSuite> TestSuites { get; init; } = new List<TestSuite>();
    
    public Guid Id { get; set; }
    
    public string? Computer { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? User { get; set; }


    public int TotalTests => TestSuites.Sum(a => a.Total);
    public int TotalErrors => TestSuites.Sum(a => a.Errors);
    public int TotalFailures => TestSuites.Sum(a => a.Failed);
    public int TotalSuccessful => TestSuites.Sum(a => a.Passed);
    
    public TimeSpan TotalTime => TimeSpan.FromSeconds((double)TestSuites.Sum(a => a.Time));
    
    public decimal SuccessRate =>
        TotalTests switch
        {
            > 0 => (100.0M * TotalSuccessful) / TotalTests,
            _ => 0
        };
}