# Dependency Injection

Cosmonaut also has a separate package that adds extensions on top of the .NET Standard Dependency injection framework.

Nuget package: [Cosmonaut.Extensions.Microsoft.DependencyInjection](https://www.nuget.org/packages/Cosmonaut.Extensions.Microsoft.DependencyInjection/)

Installing this package will add a set of methods for `IServiceCollection` called `AddCosmosStore`.

```c#
var cosmosSettings = new CosmosStoreSettings("<<databaseName>>", "<<cosmosUri>>", "<<authkey>>");
                
serviceCollection.AddCosmosStore<Book>(cosmosSettings);

// or override the collection name

serviceCollection.AddCosmosStore<Book>(cosmosSettings, "myCollection");

//or just by using the Action extension

serviceCollection.AddCosmosStore<Book>("<<databaseName>>", "<<cosmosUri>>", "<<authkey>>", settings =>
{
    settings.ConnectionPolicy = connectionPolicy;
    settings.DefaultCollectionThroughput = 5000;
    settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1),
        new RangeIndex(DataType.String, -1));
});
```