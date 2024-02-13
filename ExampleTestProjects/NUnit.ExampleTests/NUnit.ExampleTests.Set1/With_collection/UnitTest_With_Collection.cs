namespace NUnit.ExampleTests.Set1.With_collection;

[TestFixture()]
public class UnitTest_With_Collection
{
    [Test]
    public void Failing_test()
    {
        Assert.Fail("Just to test failing test with collection");
    }
    
    [Test]
    public void Successful_test()
    {
        Assert.Pass();
    }
}