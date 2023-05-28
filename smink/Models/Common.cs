using System.Text.Encodings.Web;

namespace smink.Models;

public static class Common
{
    public static string Sanitize (string name) => UrlEncoder.Default.Encode(name.Replace("%20", "_"));
}