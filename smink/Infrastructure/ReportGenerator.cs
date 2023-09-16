using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using smink.Templates;

namespace smink.Infrastructure;

public class ReportGenerator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;

    public ReportGenerator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
    }

    public async Task<string> GenerateReport(ReportConfig config)
    {
        var inputFiles = config.InputFiles;
        var outputFile = config.OutputFile;

        // If we only get one file, see if it's a pattern that hasn't been parsed by the shell
        if (inputFiles is { Length: 1 })
        {
            var path = inputFiles[0];
            var root = Directory.GetParent(path)!.FullName;
            var pattern = Path.GetFileName(path);
            inputFiles = Directory.EnumerateFiles(root, pattern).ToArray();
        }

        var testReport = await new TestReportGenerator(config.Title).GenerateReport(inputFiles);

        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);

        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "TestReport", testReport }
            };
    
            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await htmlRenderer.RenderComponentAsync<DefaultTestReportTemplate>(parameters);
            return output.ToHtmlString();
        });

        //await File.WriteAllTextAsync(outputFile, html);
        return html;
    }
    
}