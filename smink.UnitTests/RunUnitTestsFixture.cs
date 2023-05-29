using System.Diagnostics;

namespace smink.UnitTests;

public class RunUnitTestsFixture: IAsyncLifetime
{
    public string ExampleTestProjectFolder { get; }
    public string TestResultsFolder { get; }
    public IEnumerable<string> TestResultsFiles => Directory.GetFiles(TestResultsFolder, "*.test_result.xml");
    
    private string _testResultsFilePattern { get; set; }

    public RunUnitTestsFixture()
    {
        var here = Path.GetFullPath(Directory.GetCurrentDirectory());
        ExampleTestProjectFolder =  Path.GetFullPath("../../../../ExampleTestProjects/xunit.ExampleTests", here);
        TestResultsFolder = Path.Combine(ExampleTestProjectFolder, "TestResults");

        if (!Directory.Exists(TestResultsFolder))
        {
            Directory.CreateDirectory(TestResultsFolder);
        }
        
        _testResultsFilePattern = Path.Combine(TestResultsFolder, "{assembly}.test_result.xml");
    }


    public async Task InitializeAsync()
    {
        Console.WriteLine("\n\n--------------------------------------------------");
        Console.WriteLine("Running example xUnit test assembly in: " + ExampleTestProjectFolder + "\n");

        var pi = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = ExampleTestProjectFolder,
            WindowStyle = ProcessWindowStyle.Normal,
            CreateNoWindow = false,
            UseShellExecute = false,
            ArgumentList =
            {
                "test",
                "-v:q",
                $"--logger:xunit;LogFilePath={_testResultsFilePattern}"
            }
        };

        var process = Process.Start(pi);
        if (process is { })
        {
            await process.WaitForExitAsync();
        }
        
        Console.WriteLine("\nDone.");
        Console.WriteLine("--------------------------------------------------\n\n");
    }

    public Task DisposeAsync()
    {
        //Directory.Delete(TestResultsFolder, recursive: true);
        return Task.CompletedTask;
    }
}