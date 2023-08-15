using System.Globalization;
using FluentAssertions;
using FluentAssertions.Execution;
using smink.Models.NUnit;
using smink.Models.XUnit;

namespace smink.UnitTests.TestSuites.NUnit.TestResultReaders;

[Collection(nameof(RunNUnitTestsFixture))]
public class TestSuites_
{
    private readonly IEnumerable<TestRun> _results;
    private readonly TestSuite _suite;

    public TestSuites_(RunNUnitTestsFixture fixture)
    {
        _results = fixture.TestRuns;
        _suite = _results.First().TestSuites.First();
    }

    [Fact]
    public void Is_not_null()
    {
        _suite.Should().NotBeNull();
    }
    
    [Fact]
    public void Top_level_is_of_type_Assembly()
    {
        _suite.Type.Should().Be("Assembly");
    }
    
    [Fact]
    public void Next_levels_are_of_type_TestSuite()
    {
        //_suite.TestSuites.First().Type.Should().Be("TestSuite");
        foreach (var s in _suite.TestSuites)
        {
            var suite = s;
            while (suite.TestSuites.Any())
            {
                suite.Type.Should().Be("TestSuite");
                suite = suite.TestSuites.First();
            }
        }
    }
    
    [Fact]
    public void Leaf_level_is_of_type_TestFixture()
    {
        TestSuite leaf = _suite;
        while (leaf.TestSuites.Any())
        {
            leaf = leaf.TestSuites.First();
        }

        leaf.Type.Should().Be("TestFixture");
    }
    
    [Fact]
    public void Have_Duration()
    {
        _suite.Duration.Should().NotBeNull();
    }
   

    
}