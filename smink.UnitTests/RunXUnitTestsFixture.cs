using System.Diagnostics;
using smink.Models.Report;
using smink.Models.XUnit;
using smink.TestResultAdapters;
using smink.TestResultReaders;

namespace smink.UnitTests;

[CollectionDefinition(nameof(RunXUnitTestsFixture))]
public class RunXUnitTestsFixtureCollection : ICollectionFixture<RunXUnitTestsFixture>
{ }

public class RunXUnitTestsFixture: IAsyncLifetime
{
    public string ExampleTestProjectFolder { get; }
    public string TestResultsFolder { get; }
    public IEnumerable<string> TestResultsFiles => Directory.GetFiles(TestResultsFolder, "*.xunit_test_result.xml");
    public Assemblies? Assemblies { get; set; }
    public TestReport? TestReport { get; set; }

    private readonly string _testResultsFilePattern;
    private readonly XUnitResultsReader _xunit;
    private readonly XUnitResultsAdapter _xUnitResultsAdapter;

    public RunXUnitTestsFixture()
    {
        var here = Path.GetFullPath(Directory.GetCurrentDirectory());
        
        ExampleTestProjectFolder = Path.GetFullPath("xUnit.ExampleTests", FindExampleTestProjectFolder(here));
        TestResultsFolder = Path.Combine(ExampleTestProjectFolder, "TestResults");

        if (!Directory.Exists(TestResultsFolder))
        {
            Directory.CreateDirectory(TestResultsFolder);
        }
        
        _testResultsFilePattern = Path.Combine(TestResultsFolder, "{assembly}.xunit_test_result.xml");
        
        _xunit = new XUnitResultsReader();
        _xUnitResultsAdapter = new XUnitResultsAdapter();
    }
  
    public async Task InitializeAsync()
    {
        await RunXunitTests();
        await LoadResults();
        LoadTestReportData();
    }

    private void LoadTestReportData()
    {
       TestReport = _xUnitResultsAdapter.Read(Assemblies);
    }

    private async Task LoadResults()
    {
        Assemblies = await _xunit.Load(TestResultsFiles.ToArray());
    }


    private async Task RunXunitTests()
    {
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
        }
    }

    private static string FindExampleTestProjectFolder(string here)
    {
        const string folderToFind = "ExampleTestProjects";

        var currentFolder = new DirectoryInfo(here);

        bool found;
        do
        {
            currentFolder = currentFolder.Parent;
            var directories = currentFolder!.GetDirectories(folderToFind);
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