using System.Globalization;
using FluentAssertions;
using smink.TestResultReaders;

namespace smink.UnitTests.TestSuites.TestResultReaders_xUnit;

public class Assemblies_: IClassFixture<RunUnitTestsFixture>
{
    private readonly RunUnitTestsFixture _fixture;
    private readonly XUnitResultsReader _xunit;

    public Assemblies_(RunUnitTestsFixture fixture)
    {
        _fixture = fixture;
        _xunit = new XUnitResultsReader();
    }

    [Fact]
    public async Task Is_not_null()
    {
        var results = await _xunit.Load(_fixture.TestResultsFiles.ToArray());
        results.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Contains_each_single_Assembly()
    {
        var results = await _xunit.Load(_fixture.TestResultsFiles.ToArray());
        results!.AssembliesList.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task Has_Timestamp()
    {
        var results = await _xunit.Load(_fixture.TestResultsFiles.ToArray());
        results!.Timestamp!.Should().NotBeNull();
        var dt = DateTime.Parse(results.Timestamp!, DateTimeFormatInfo.InvariantInfo);
        dt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(15));
    }

    
}