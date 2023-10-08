namespace NUnit.ExampleTests.Set1.With_collection;

[TestFixture()]
public class UnitTest_With_Collection
{
    [Test]
    public void Failing_test()
    {
        Assert.True(false);
    }
    
    [Test]
    public void Successful_test()
    {
        Assert.True(true);
    }
}