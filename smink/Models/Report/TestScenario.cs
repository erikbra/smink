namespace smink.Models.Report;

public class TestScenario
{
    public IEnumerable<TestRun> Tests { get; init; } = new List<TestRun>();
    
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    
    public int Passed { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public int Failed { get; set; }
    public int NotRun { get; set; }
    public int Total { get; set; }
    
    public decimal Time { get; set; }

    public int Minutes => ((int) Math.Floor(Time)) / 60;
    public int Seconds => ((int) Math.Round(Time)) % 60;
    
    public decimal SuccessRate =>
        Total switch
        {
            > 0 => (100.0M * Passed) / Total,
            _ => 0
        };

}