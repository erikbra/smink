namespace smink.Models.Report;

public class TestReport
{
    public IEnumerable<TestSuite> TestSuites { get; init; } = new List<TestSuite>();
    
    public string? Id { get; set; }
    public string Title { get; set; } = "Test Result";
    
    public string? Computer { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? User { get; set; }


    public int TotalTests { get; init; }
    public int TotalErrors { get; init; }
    public int TotalFailures { get; init; }
    public int TotalSuccessful { get; init; }

    public TimeSpan TotalTime => TimeSpan.FromSeconds((double)TestSuites.Sum(a => a.Time));
    
    public decimal SuccessRate =>
        TotalTests switch
        {
            > 0 => (100.0M * TotalSuccessful) / TotalTests,
            _ => 0
        };

}