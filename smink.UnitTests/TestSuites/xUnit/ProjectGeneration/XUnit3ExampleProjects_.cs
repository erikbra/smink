using AwesomeAssertions;
using System.Xml.Linq;

namespace smink.UnitTests.TestSuites.xUnit.ProjectGeneration;

[Collection(nameof(RunXUnit3TestsFixture))]
public class XUnit3ExampleProjects_
{
    private readonly RunXUnit3TestsFixture _fixture;

    public XUnit3ExampleProjects_(RunXUnit3TestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Generates_test_result_files_for_both_projects()
    {
        var results = _fixture.TestResultsFiles.ToArray();

        results.Should().HaveCount(2);
        results.Should().Contain(f => f.EndsWith("xUnit3.ExampleTests.Set1.trx"));
        results.Should().Contain(f => f.EndsWith("xUnit3.ExampleTests.Set2.trx"));
    }

    [Fact]
    public void Generates_non_empty_trx_with_test_results()
    {
        foreach (var file in _fixture.TestResultsFiles)
        {
            var doc = XDocument.Load(file);
            doc.Descendants().Any(e => e.Name.LocalName == "UnitTestResult").Should().BeTrue();
        }
    }
}
