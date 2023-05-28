using System.Text.Encodings.Web;
using System.Xml.Serialization;

namespace smink.Models.XUnit;

public record Test
{
    [XmlAttribute("id")]
    public Guid Id { get; set;  }
    
    [XmlAttribute("type")]
    public string? Type { get; set; }
    
    [XmlAttribute("method")]
    public string? Method { get; set; }
    [XmlAttribute("name")]
    public string? Name { get; set; }
    
    [XmlAttribute("result")]
    public Result? Result { get; set; }
    
    [XmlAttribute("source-file")]
    public string? SourceFile { get; set; }
    [XmlAttribute("source-line")]
    public string? SourceLine { get; set; }
    
    [XmlAttribute("time")]
    public decimal Time { get; set; }
    [XmlAttribute("time-rtf")]
    public TimeSpan TimeRtf { get; set; }
    
    [XmlElement("failure")]
    public Failure? Failure { get; set; }
    
    [XmlElement("output")]
    public string? Output { get; set; }
    [XmlElement("reason")]
    public string? Reason { get; set; }
    
    [XmlArray("traits")]
    public List<Trait>? Traits { get; set; }

    public bool HasContent => Failure is {} || Reason is {} || Output is {};
    public string SanitizedName => UrlEncoder.Default.Encode(Name ?? Id.ToString());


}