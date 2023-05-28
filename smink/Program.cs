using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using smink.Templates;
using smink.TestResultAdapters;
using smink.TestResultReaders;

IServiceCollection services = new ServiceCollection();
services.AddLogging();

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

IServiceProvider serviceProvider = services.BuildServiceProvider();
ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

var inputFiles = args[..^1];
var outputFile = args[^1];

// If we only get one file, see if it's a pattern that hasn't been parsed by the shell
if (inputFiles is { Length: 1 })
{
    var path = inputFiles[0];
    var root = Directory.GetParent(path)!.FullName;
    var pattern = Path.GetFileName(path);
    inputFiles = Directory.EnumerateFiles(root, pattern).ToArray();
}

var xunit = new XUnitResultsReader();
var testResults = await xunit.Load(inputFiles);
var testReport = new XUnitResultsAdapter().Read(testResults);

await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);

var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
{
    var dictionary = new Dictionary<string, object?>
    {
        { "TestReport", testReport }
    };
    
    var parameters = ParameterView.FromDictionary(dictionary);
    var output = await htmlRenderer.RenderComponentAsync<XUnitTemplate>(parameters);
    return output.ToHtmlString();
});

File.WriteAllText(outputFile, html);

//Console.WriteLine(html);
