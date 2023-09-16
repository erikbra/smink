using System.Xml.Linq;
using smink.Models.NUnit;

using static smink.TestResultReaders.XmlUtils;

namespace smink.TestResultReaders;

//https://docs.nunit.org/articles/nunit/technical-notes/usage/Test-Result-XML-Format.html

public class NUnitResultsReader
{
    public Task<IEnumerable<TestRun>> Load(IEnumerable<string> files) => Load(files.ToArray());
    
    public async Task<IEnumerable<TestRun>> Load(string[] files)
    {
        var ct = new CancellationToken();
        
        var testRuns = await Task.WhenAll(files.Select(file => ReadFile(file, ct)));
        return testRuns
            .Where(tr => tr is { })
            .Cast<TestRun>();
    }

    private static async Task<TestRun?> ReadFile(string file, CancellationToken ct)
    {
        await using var stream = File.OpenRead(file);
        var doc = await XDocument.LoadAsync(stream, LoadOptions.PreserveWhitespace, ct);
        var testRun = Parse(doc);
        return testRun;
    }

    private static TestRun? Parse(XDocument doc)
    {
        return doc.Root switch
        {
            { } root => new TestRun()
            {
                Id = GetAttribute<string>(root, "id"),
                Duration = GetAttribute<decimal>(root, "duration"),
                TestCaseCount = GetAttribute<int>(root, "testcasecount"),
                Total = GetAttribute<int>(root, "total"),
                Passed = GetAttribute<int>(root, "passed"),
                Failed = GetAttribute<int>(root, "failed"),
                Inconclusive = GetAttribute<int>(root, "inconclusive"),
                Skipped = GetAttribute<int>(root, "skipped"),
                Result = GetAttribute<string>(root, "result"),
                StartTime = GetAttribute<DateTime>(root, "start-time"),
                EndTime = GetAttribute<DateTime>(root, "end-time"),
                
                TestSuites = GetTestSuites(root)
                
            },
            _ => null
        };
    }

    private static IEnumerable<TestSuite> GetTestSuites(XElement root)
    {
        return root.Elements(XName.Get("test-suite")).Select(element => new TestSuite()
        {
            Type = GetAttribute<string>(element, "type"),
            Name = GetAttribute<string>(element, "name"),
            FullName = GetAttribute<string>(element, "fullname"),
            
            Total = GetAttribute<int>(element, "total"),
            Passed = GetAttribute<int>(element, "passed"),
            Failed = GetAttribute<int>(element, "failed"),
            Inconclusive = GetAttribute<int>(element, "inconclusive"),
            Skipped = GetAttribute<int>(element, "skipped"),
            Result = GetAttribute<string>(element, "result"),
            StartTime = GetAttribute<DateTime>(element, "start-time"),
            EndTime = GetAttribute<DateTime>(element, "end-time"),
            Duration = GetAttribute<decimal>(element, "duration"),
            
            TestSuites = GetTestSuites(element),
            TestCases = GetTestCases(element)
        });
       
    }
    
    private static IEnumerable<TestCase> GetTestCases(XElement root)
    {
        return root.Elements(XName.Get("test-case")).Select(element => new TestCase()
        {
            Name = GetAttribute<string>(element, "name"),
            FullName = GetAttribute<string>(element, "fullname"),
            ClassName = GetAttribute<string>(element, "classname"),
            MethodName = GetAttribute<string>(element, "methodname"),
            
            Total = GetAttribute<int>(element, "total"),
            Passed = GetAttribute<int>(element, "passed"),
            Failed = GetAttribute<int>(element, "failed"),
            Inconclusive = GetAttribute<int>(element, "inconclusive"),
            Skipped = GetAttribute<int>(element, "skipped"),
            Result = GetAttribute<string>(element, "result"),
            StartTime = GetAttribute<DateTime>(element, "start-time"),
            EndTime = GetAttribute<DateTime>(element, "end-time"),
            Duration = GetAttribute<decimal>(element, "duration"),
            
            Output = element.Elements(XName.Get("output")).SingleOrDefault()?.Value,
            
            Failures = GetFailures(element)
            
        });
       
    }
    private static IEnumerable<Failure> GetFailures(XElement root)
    {
        return root.Elements(XName.Get("failure")).Select(element => new Failure()
        {
            Message = element.Element(XName.Get("message"))?.Value,
            StackTrace =  element.Element(XName.Get("stack-trace"))?.Value
        });
    }
}