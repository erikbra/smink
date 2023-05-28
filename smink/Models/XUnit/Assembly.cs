using System.Xml.Serialization;

namespace smink.Models.XUnit;

public record Assembly()
{
    [XmlAttribute("id")]
    public Guid Id { get; set;  }
    
    [XmlAttribute("name")]
    public string? Name { get; set; }
    
    [XmlAttribute("config-file")]
    public string? ConfigFile { get; set; }
    
    [XmlAttribute("environment")]
    public string? Environment { get; set; }
    
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
    
    [XmlAttribute("start-rtf")]
    public DateTime StartRtf { get; set; }
    
    [XmlAttribute("finish-rtf")]
    public DateTime FinishRtf { get; set; }
    
    [XmlAttribute("time-rtf")]
    public TimeSpan TimeRtf { get; set; }
    
    [XmlAttribute("time")]
    public decimal Time { get; set; }
    
    [XmlAttribute("run-date")]
    public string? RunDate { get; set; }
    
    [XmlAttribute("run-time")]
    public string? RunTime { get; set; }
    
    [XmlAttribute("target-framework")]
    public string? TargetFramework { get; set; }
    
    [XmlAttribute("test-framework")]
    public string? TestFramework { get; set; }

    [XmlElement("collection")] 
    public List<Collection> Collections { get; set; } = new List<Collection>();
    
    
    [XmlElement("errors")]
    public List<Error>? ErrorsList { get; set; }
    
    public int TotalTests => Collections.Sum(c => c.Total);
    public int TotalErrors => Collections.Sum(c => c.Errors);
    public int TotalFailures => Collections.Sum(c => c.Failed);
    public int TotalSuccessful => Collections.Sum(c => c.Passed);
    public int TotalSkipped => Collections.Sum(c => c.Skipped);
    
    public decimal TotalTime => Collections.Sum(c => c.Time);
    public int TotalMinutes => Collections.Sum(c => ((int) Math.Floor(c.Time)) / 60);
    public int TotalSeconds => Collections.Sum(c => ((int) Math.Round(c.Time)) % 60);

    public decimal SuccessRate =>
        TotalTests switch
        {
            > 0 => (100.0M * TotalSuccessful) / TotalTests,
            _ => 0
        };


}