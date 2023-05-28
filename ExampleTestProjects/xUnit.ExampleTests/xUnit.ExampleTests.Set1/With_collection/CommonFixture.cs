namespace xUnit.ExampleTests.Set1.With_collection;

public class CommonFixture
{
    
}

[CollectionDefinition("Common fixture")]
public class DatabaseCollection : ICollectionFixture<CommonFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}