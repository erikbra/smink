using System.Diagnostics;

namespace smink.UnitTests;

[CollectionDefinition(nameof(RunXUnit3TestsFixture))]
public class RunXUnit3TestsFixtureCollection : ICollectionFixture<RunXUnit3TestsFixture>
{ }

public class RunXUnit3TestsFixture: IAsyncLifetime
{
    public string ExampleTestProjectFolder { get; }
    public string TestResultsFolder { get; }
    public IEnumerable<string> TestResultsFiles => Directory.GetFiles(TestResultsFolder, "*.trx");

    public RunXUnit3TestsFixture()
    {
        var here = Path.GetFullPath(Directory.GetCurrentDirectory());

        ExampleTestProjectFolder = Path.GetFullPath("xUnit3.ExampleTests", FindExampleTestProjectFolder(here));
        TestResultsFolder = Path.Combine(ExampleTestProjectFolder, "TestResults");

        if (!Directory.Exists(TestResultsFolder))
        {
            Directory.CreateDirectory(TestResultsFolder);
        }

        foreach (var file in Directory.GetFiles(TestResultsFolder, "*.trx"))
        {
            File.Delete(file);
        }
    }

    public async Task InitializeAsync()
    {
        await RunXunitTests();
    }

    private async Task RunXunitTests()
    {
        await RunProjectTests(
            "xUnit3.ExampleTests.Set1/xUnit3.ExampleTests.Set1.csproj",
            "xUnit3.ExampleTests.Set1.trx");

        await RunProjectTests(
            "xUnit3.ExampleTests.Set2/xUnit3.ExampleTests.Set2.csproj",
            "xUnit3.ExampleTests.Set2.trx");
    }

    private async Task RunProjectTests(string projectPath, string trxFileName)
    {
        var pi = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = ExampleTestProjectFolder,
            WindowStyle = ProcessWindowStyle.Normal,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            ArgumentList =
            {
                "test",
                projectPath,
                "-verbosity:q",
                "-maxcpucount:1",
                "--results-directory",
                TestResultsFolder,
                $"--logger:trx;LogFileName={trxFileName}"
            }
        };

        var process = Process.Start(pi) ?? throw new ApplicationException("Unable to start dotnet test process");
        await process.WaitForExitAsync();

        // Example projects intentionally include failing tests, so non-zero is expected.
        // The fixture's responsibility is to generate result files for report/test parsing.
        _ = process.ExitCode;
    }

    private static string FindExampleTestProjectFolder(string here)
    {
        const string folderToFind = "ExampleTestProjects";

        var currentFolder = new DirectoryInfo(here);

        bool found;
        do
        {
            currentFolder = currentFolder.Parent ?? throw new ApplicationException($"Reached filesystem root without finding {folderToFind}");
            var directories = currentFolder.GetDirectories(folderToFind);
            found = directories.Length != 0;
        } while (!found && currentFolder != currentFolder.Root);

        if (!found)
        {
            throw new ApplicationException($"Unable to find folder {folderToFind} in parents of {here}");
        }

        return Path.GetFullPath(folderToFind, currentFolder.FullName);
    }


    public Task DisposeAsync()
    {
        //Directory.Delete(TestResultsFolder, recursive: true);
        return Task.CompletedTask;
    }
}
