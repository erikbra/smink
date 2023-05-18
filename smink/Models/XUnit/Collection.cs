using System.Xml.Serialization;

namespace smink.Models.XUnit;

public record Collection
{
    [XmlAttribute("id")]
    public Guid Id { get; set; }
    
    [XmlAttribute("name")]
    public string? Name { get; set; }
    
    [XmlAttribute("passed")]
    public int Passed { get; set; }
    [XmlAttribute("skipped")]
    public int Skipped { get; set; }
    [XmlAttribute("errors")]
    public int Errors { get; set; }
    [XmlAttribute("failed")]
    public int Failed { get; set; }
    [XmlAttribute("not-run")]
    public int NotRun { get; set; }
    [XmlAttribute("total")]
    public int Total { get; set; }
    
    [XmlAttribute("time-rtf")]
    public TimeSpan TimeRtf { get; set; }
    [XmlAttribute("time")]
    public decimal Time { get; set; }

    [XmlElement("test")] public List<Test> Tests { get; set; } = new List<Test>();
    
    // public int TotalTests => Tests.Count;
    // public int TotalErrors => Tests.Count(c => c.Result == Result.Fail);
    // public int TotalFailures => Tests.Count(c => c.Failure is {});
    // public int TotalSuccessful => Tests.Count(c => c.Result == Result.Pass);
    // public int TotalSkipped => Tests.Count(c => c.Result == Result.Pass);
    //
    // public decimal TotalTime => Tests.Sum(t => t.Time);
    
    public int Minutes => ((int) Math.Floor(Time)) / 60;
    public int Seconds => ((int) Math.Round(Time)) % 60;
    
    public decimal SuccessRate =>
        Total switch
        {
            > 0 => (100.0M * Passed) / Total,
            _ => 0
        };
    

}