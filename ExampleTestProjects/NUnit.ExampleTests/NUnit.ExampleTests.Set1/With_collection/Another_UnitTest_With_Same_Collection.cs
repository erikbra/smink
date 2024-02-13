namespace NUnit.ExampleTests.Set1.With_collection;

[TestFixture()]
// ReSharper disable once InconsistentNaming
public class Another_UnitTest_With_Same_Fixture
{
    [Test]
    public void Failing_test()
    {
        Assert.Fail("Just to test failing test in Set 1");
    }
    
    [Test]
    public void Successful_test()
    {
        Assert.Pass();
    }
}