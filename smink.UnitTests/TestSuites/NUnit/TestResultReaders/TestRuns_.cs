using System.Globalization;
using FluentAssertions;
using FluentAssertions.Execution;
using smink.Models.NUnit;
using smink.Models.XUnit;

namespace smink.UnitTests.TestSuites.NUnit.TestResultReaders;

[Collection(nameof(RunNUnitTestsFixture))]
public class TestRuns_
{
    private readonly IEnumerable<TestRun> _results;

    public TestRuns_(RunNUnitTestsFixture fixture)
    {
        _results = fixture.TestRuns;
    }

    [Fact]
    public void Is_not_null()
    {
        _results.Should().NotBeNull();
    }
    
    [Fact]
    public void Contains_each_single_TestRun()
    {
        _results!.Should().HaveCount(2);
    }
    
      
    [Fact]
    public void Contains_TestSuites()
    {
        _results!.First().TestSuites.Should().HaveCount(1);
    }
    
    [Fact]
    public void Have_Duration()
    {
        _results!.First().Duration.Should().NotBeNull();
    }
    
    [Fact]
    public void Suites_have_Duration()
    {
        _results.Should().AllSatisfy(r => r.TestSuites.Should().AllSatisfy(s => s.Duration.Should().NotBeNull()));
    }
    
    [Fact]
    public void Have_all_Timestamp()
    {
        using (new AssertionScope())
        {
            foreach (var testRun in _results)
            {
                testRun.StartTime.Should().NotBeNull();
                //var dt = DateTime.Parse(_results.Timestamp!, DateTimeFormatInfo.InvariantInfo);
                var dt = testRun.StartTime;
                dt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(15));
            }
        }
    }

    
}