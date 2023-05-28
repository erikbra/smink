using System.Xml.Serialization;

namespace smink.Models.XUnit;

public record Error
{
    [XmlAttribute("name")]
    public string? Name { get; set; }
    [XmlAttribute("type")]
    public ErrorType? Type { get; set; }
    
    [XmlElement("failure")]
    public Failure? Failure { get; set; }
}