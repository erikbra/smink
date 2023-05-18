using Microsoft.AspNetCore.Components;
using smink.Models.Report;

namespace smink.Templates;

public class XUnitTemplateRazorPage: ComponentBase
{
    [Parameter] public TestReport TestReport { get; set; } = new();
    
    protected static string GetStyle(TestRun test)
    {
        var prefix = test switch {
            { Failure: { } }  => "color:#212121; background-color:rgb(255, 156, 156)",
            { Result: "Fail"} => "color:#212121; background-color:#ffa468;",
            { Result: "Skip"} => "color:#808080; background-color:#e8e8e8",
            _ => "color:#212121; background-color:#baf3b7;"
        };

        var suffix = !test.HasContent ? "cursor:default;" : "";
        return $"{prefix};{suffix}";
    }
    
    protected static string? Shorten(string? message) => message switch{
        { Length: > 2000} => message[..1999] + "...",
        _ => message 
    };


}