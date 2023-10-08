using System.Collections;

namespace smink.Models.NUnit;

public record TestCase
{
    public string? Name { get; set; }
    public string? FullName { get; set; }
    public string? ClassName { get; set; }
    public string? MethodName { get; set; }
    
    public decimal? Duration { get; set; }
    public int? Total { get; set; }
    public int? Passed { get; set; }
    public int? Failed { get; set; }
    public int? Inconclusive { get; set; }
    public int? Skipped { get; set; }
    public string? Result { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public IEnumerable<Failure> Failures { get; set; } = Enumerable.Empty<Failure>();
    public string? Output { get; set; }
    
}