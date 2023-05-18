namespace xUnit.ExampleTests.Set1.With_collection;

[Collection("Common fixture")]
public class UnitTest_With_Collection
{
    [Fact]
    public void Failing_test()
    {
        Assert.True(false);
    }
    
    [Fact]
    public void Successful_test()
    {
        Assert.True(true);
    }
}