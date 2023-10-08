namespace smink.Models.NUnit;

public record TestSuite
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? FullName { get; set; }
    
    public decimal? Duration { get; set; }
    public int? Total { get; set; }
    public int? Passed { get; set; }
    public int? Failed { get; set; }
    public int? Inconclusive { get; set; }
    public int? Skipped { get; set; }
    public string? Result { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public IEnumerable<TestSuite> TestSuites { get; set; } = Enumerable.Empty<TestSuite>();
    public IEnumerable<TestCase> TestCases { get; set; } = Enumerable.Empty<TestCase>();
    
}