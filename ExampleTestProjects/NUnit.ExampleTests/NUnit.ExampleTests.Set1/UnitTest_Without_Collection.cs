namespace NUnit.ExampleTests.Set1;

public class UnitTest_Without_Collection
{
    [Test]
    public void Failing_Test()
    {
        Assert.Fail();
    }
    
    [Ignore("Just to test ignoring")]
    public void Ignored_Test()
    {
        Assert.Fail();
    }
}