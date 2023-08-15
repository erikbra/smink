using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using smink;
using smink.Infrastructure;
using smink.Templates;

IServiceCollection services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<ReportGenerator>();

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

IServiceProvider serviceProvider = services.BuildServiceProvider();
//ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

var rootCommand = new RootCommand();

var input = CommandLineArguments.InputFiles();
var output = CommandLineArguments.OutputFile();
var titleArgument = CommandLineArguments.Title();

rootCommand.Add(input);
rootCommand.Add(output);
rootCommand.Add(titleArgument);

rootCommand.SetHandler(async (inputFiles, outputFiles, title) =>
{
    var config = new ReportConfig()
    {
        InputFiles = new[] { inputFiles },
        OutputFile = outputFiles,
        Title = title
    };

    var rg = serviceProvider.GetRequiredService<ReportGenerator>();
    var html = await rg.GenerateReport(config);
    await File.WriteAllTextAsync(outputFiles, html);

}, input, output, titleArgument);

await rootCommand.InvokeAsync(args);

// var inputFiles = args[..^1];
// var outputFile = args[^1];
