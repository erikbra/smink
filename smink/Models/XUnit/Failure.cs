using System.Xml.Serialization;

namespace smink.Models.XUnit;

public record Failure
{
    [XmlAttribute("exception-type")] 
    public string? ExceptionType { get; set; }

    [XmlElement("message")] 
    public string? Message { get; set; }

    [XmlElement("stack-trace")] 
    public string? StackTrace { get; set; }
}