namespace NUnit.ExampleTests.Set1.With_collection;

[TestFixture()]
// ReSharper disable once InconsistentNaming
public class Another_UnitTest_With_Same_Fixture
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