using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using smink;
using smink.Infrastructure;
using smink.Templates;

if (args.Length < 2)
{
    Console.WriteLine("Usage: smink <input-file> <output-file> [--title <title>]");
    return 1;
}

IServiceCollection services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<ReportGenerator>();

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

IServiceProvider serviceProvider = services.BuildServiceProvider();

var inputFile = args[0];
var outputFile = args[1];
var title = "Test Result";

// Parse optional --title argument
for (int i = 2; i < args.Length - 1; )
{
    if ((args[i] == "--title" || args[i] == "-t") && i + 1 < args.Length)
    {
        title = args[i + 1];
        i += 2;
    }
    else
    {
        i++;
    }
}

try
{
    var config = new ReportConfig()
    {
        InputFiles = new[] { inputFile },
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
