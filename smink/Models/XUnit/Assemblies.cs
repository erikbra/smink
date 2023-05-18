using System.Xml.Serialization;

namespace smink.Models.XUnit;

[XmlRoot(ElementName = "assemblies")]
public class Assemblies
{
    public Assemblies() {}
    
    [XmlAttribute("schema-version")]
    public int SchemaVersion { get; set; }
    
    [XmlAttribute("computer")]
    public string? Computer { get; set; }
    
    [XmlAttribute("start-rtf")]
    public DateTime StartRtf { get; set; }
    
    [XmlAttribute("finish-rtf")]
    public DateTime? FinishRtf { get; set; }
    
    [XmlAttribute("timestamp")]
    public string? Timestamp { get; set; }
    
    [XmlAttribute("user")]
    public string? User { get; set; }
    
    [XmlAttribute("id")]
    public Guid Id { get; set; }

    [XmlElement("assembly")] 
    public List<Assembly> AssembliesList { get; set; } = new();

    public int TotalTests => AssembliesList.Sum(a => a.Total);
    public int TotalErrors => AssembliesList.Sum(a => a.Errors);
    public int TotalFailures => AssembliesList.Sum(a => a.Failed);
    public int TotalSuccessful => AssembliesList.Sum(a => a.Passed);
    public decimal TotalTime => AssembliesList.Sum(a => a.Time);
    
    public int TotalMinutes => AssembliesList.Sum(a => ((int) Math.Floor(a.Time)) / 60);
    public int TotalSeconds => AssembliesList.Sum(a => ((int) Math.Round(a.Time)) % 60);

    public decimal SuccessRate =>
        TotalTests switch
        {
            > 0 => (100.0M * TotalSuccessful) / TotalTests,
            _ => 0
        };

}