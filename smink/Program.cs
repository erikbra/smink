using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using smink;
using smink.Infrastructure;
using smink.Templates;

// Parse optional named arguments and collect positional args
var positionalArgs = new List<string>();
var title = "Test Result";

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--title" || args[i] == "-t")
    {
        if (i + 1 >= args.Length)
        {
            Console.Error.WriteLine($"Error: '{args[i]}' requires a value.");
            Console.Error.WriteLine("Usage: smink <input-file> [<input-file> ...] <output-file> [--title <title>]");
            return 1;
        }
        title = args[i + 1];
        i++;
    }
    else
    {
        positionalArgs.Add(args[i]);
    }
}

if (positionalArgs.Count < 2)
{
    Console.Error.WriteLine("Usage: smink <input-file> [<input-file> ...] <output-file> [--title <title>]");
    return 1;
}

var inputFiles = positionalArgs.Take(positionalArgs.Count - 1).ToArray();
var outputFile = positionalArgs[^1];

IServiceCollection services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<ReportGenerator>();

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

IServiceProvider serviceProvider = services.BuildServiceProvider();

try
{
    var config = new ReportConfig()
    {
        InputFiles = inputFiles,
        OutputFile = outputFile,
        Title = title
    };

    var rg = serviceProvider.GetRequiredService<ReportGenerator>();
    var html = await rg.GenerateReport(config);
    await File.WriteAllTextAsync(outputFile, html);
    Console.WriteLine($"Report generated: {outputFile}");
    return 0;
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Error: {ex.Message}");
    return 1;
}
