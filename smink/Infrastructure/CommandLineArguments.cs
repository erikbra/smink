using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.DependencyInjection;

namespace smink.Infrastructure;

public static class CommandLineArguments
{
    public static Argument<string> InputFiles() =>
        new(
            name: "input",
            description: "Input files"
        );
    
    public static Argument<string> OutputFile() =>
        new(
            name: "output",
            description: "Output file"
        );
    
    public static Option<string> Title() =>
        new(
            new[] { "--title", "-t" },
            () => "Test Result",
            "The title of the report"
        )
        { IsRequired = false };
    
}