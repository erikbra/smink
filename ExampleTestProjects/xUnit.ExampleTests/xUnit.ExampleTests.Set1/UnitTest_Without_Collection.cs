namespace xUnit.ExampleTests.Set1;

public class UnitTest_Without_Collection
{
    [Fact]
    public void Failing_Test()
    {
        Assert.True(false);
    }
    
    [Fact(Skip = "Just to test skipping")]
    public void Skipped_Test()
    {
        Assert.True(false);
    }
}