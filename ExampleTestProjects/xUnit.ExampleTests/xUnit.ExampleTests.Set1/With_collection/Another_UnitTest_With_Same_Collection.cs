namespace xUnit.ExampleTests.Set1.With_collection;

[Collection("Common fixture")]
public class Another_UnitTest_With_Same_Collection
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