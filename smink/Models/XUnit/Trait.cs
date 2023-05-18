using System.Xml.Serialization;

namespace smink.Models.XUnit;

public record Trait
{
    [XmlAttribute("name")]
    public string? Name { get; set; }
    [XmlAttribute("value")]
    public string? Value { get; set; }
}