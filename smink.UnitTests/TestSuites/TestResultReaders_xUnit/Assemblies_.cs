using System.Globalization;
using FluentAssertions;
using smink.Models.XUnit;

namespace smink.UnitTests.TestSuites.TestResultReaders_xUnit;

[Collection(nameof(RunUnitTestsFixture))]
public class Assemblies_
{
    private readonly Assemblies? _results;

    public Assemblies_(RunUnitTestsFixture fixture)
    {
        _results = fixture.Assemblies;
    }

    [Fact]
    public void Is_not_null()
    {
        _results.Should().NotBeNull();
    }
    
    [Fact]
    public void Contains_each_single_Assembly()
    {
        _results!.AssembliesList.Should().HaveCount(2);
    }
    
    [Fact]
    public void Has_Timestamp()
    {
        _results!.Timestamp!.Should().NotBeNull();
        var dt = DateTime.Parse(_results.Timestamp!, DateTimeFormatInfo.InvariantInfo);
        dt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(15));
    }

    
}