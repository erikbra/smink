using System.Diagnostics;
using smink.Models.Report;
using smink.Models.XUnit;
using smink.TestResultAdapters;
using smink.TestResultReaders;
using TestRun = smink.Models.NUnit.TestRun;

namespace smink.UnitTests;

[CollectionDefinition(nameof(RunNUnitTestsFixture))]
public class RunNUnitTestsFixtureCollection : ICollectionFixture<RunNUnitTestsFixture>
{ }

public class RunNUnitTestsFixture: IAsyncLifetime
{
    public string ExampleTestProjectFolder { get; }
    public string TestResultsFolder { get; }
    public IEnumerable<string> TestResultsFiles => Directory.GetFiles(TestResultsFolder, "*.nunit_test_result.xml");
    public IEnumerable<TestRun> TestRuns { get; set; }
    public TestReport? TestReport { get; set; }

    private readonly string _testResultsFilePattern;
    private readonly NUnitResultsReader _nunit;
    private readonly NUnitResultsAdapter _nUnitResultsAdapter;

    public RunNUnitTestsFixture()
    {
        var here = Path.GetFullPath(Directory.GetCurrentDirectory());
        
        ExampleTestProjectFolder = Path.GetFullPath("NUnit.ExampleTests", FindExampleTestProjectFolder(here));
        TestResultsFolder = Path.Combine(ExampleTestProjectFolder, "TestResults");

        if (!Directory.Exists(TestResultsFolder))
        {
            Directory.CreateDirectory(TestResultsFolder);
        }
        
        _testResultsFilePattern = Path.Combine(TestResultsFolder, "{assembly}.nunit_test_result.xml");
        
        _nunit = new NUnitResultsReader();
        _nUnitResultsAdapter = new NUnitResultsAdapter();
    }
  
    public async Task InitializeAsync()
    {
        await RunNUnitTests();
        await LoadResults();
        LoadTestReportData();
    }

    private void LoadTestReportData()
    {
       TestReport = _nUnitResultsAdapter.Read(TestRuns);
    }

    private async Task LoadResults()
    {
        TestRuns = await _nunit.Load(TestResultsFiles.ToArray());
    }


    private async Task RunNUnitTests()
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
                $"--logger:nunit;LogFilePath={_testResultsFilePattern}"
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