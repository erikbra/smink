using System.Globalization;
using System.Xml.Linq;

namespace smink.TestResultReaders;

public static class XmlUtils
{
    public static string? GetAttribute(XElement? elem, string name)
    {
        return elem?.Attribute(XName.Get(name))?.Value;
    }
    
    public static T? GetAttribute<T>(XElement? elem, string name) where T: IParsable<T>
    {
        return GetAttribute(elem, name) switch
        {
            { } val => T.TryParse(val, CultureInfo.InvariantCulture, out var s) ? s : default,
            _ => default(T?)
        };
    }
    
    public static T? GetEnumAttribute<T>(XElement? elem, string name) where T: struct, Enum
    {
        return GetAttribute(elem, name) switch
        {
            { } val => Enum.Parse<T>(val),
            _ => default
        };
    }
    
}