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
        
        ExampleTestProjectFolder = Path.GetFullPath("xunit.ExampleTests", FindExampleTestProjectFolder(here));
        TestResultsFolder = Path.Combine(ExampleTestProjectFolder, "TestResults");

        if (!Directory.Exists(TestResultsFolder))
        {
            Directory.CreateDirectory(TestResultsFolder);
        }
        
        _testResultsFilePattern = Path.Combine(TestResultsFolder, "{assembly}.test_result.xml");
    }

    private static string FindExampleTestProjectFolder(string here)
    {
        const string folderToFind = "ExampleTestProjects";

        var currentFolder = new DirectoryInfo(here);

        bool found;
        do
        {
            currentFolder = currentFolder.Parent;
            var directories = currentFolder.GetDirectories(folderToFind);
            found = directories.Any();
        } while (!found && currentFolder != currentFolder.Root);

        if (!found)
        {
            throw new ApplicationException($"Unable to find folder {folderToFind} in parents of {here}");
        }

        Console.WriteLine("Current folder: " + currentFolder.FullName);

        return Path.GetFullPath(folderToFind, currentFolder.FullName);
    }


    public async Task InitializeAsync()
    {
        Console.WriteLine("\n\n--------------------------------------------------");
        Console.WriteLine("Running example xUnit test assembly in: " + ExampleTestProjectFolder + "\n");

        var files = string.Join(' ', Directory.GetFiles(ExampleTestProjectFolder));
        Console.WriteLine("Files in " + ExampleTestProjectFolder + ": " + files);
        

        var pi = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = ExampleTestProjectFolder,
            WindowStyle = ProcessWindowStyle.Normal,
            CreateNoWindow = true,
            UseShellExecute = false,
            ArgumentList =
            {
                "test",
                "-verbosity:q",
                "-maxcpucount:1",
                $"--logger:xunit;LogFilePath={_testResultsFilePattern}"
            }
        };

        var process = Process.Start(pi);
        if (process is { })
        {
            await process.WaitForExitAsync();
            var exitCode = process.ExitCode;
//             if (exitCode != 0)
//             {
//                 throw new ApplicationException(
//                         $@"Unexpected exit code when running tests: {exitCode}.
// Working directory: {pi.WorkingDirectory}
// Command: {pi.FileName}
// Arguments: {string.Join(' ', pi.ArgumentList)}
// "
//                         );
//             }
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