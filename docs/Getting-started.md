# Getting Started Guide

## What is Cosmonaut?

Cosmonaut is a supercharged Azure CosmosD DB SDK for the SQL API with ORM support. It eliminates the need for most of the data-access code that developers usually need to write and it limits the unit of work scope to the object itself that the developer needs to work with.

## Why use Cosmonaut?

The official Cosmos DB SDK has a ton of features and it can do a lot of things, but there is no clear path when it comes to doing those things. There is object mapping but the scope always stays the same.

Cosmonaut limits the scope from the Database account level to the `CosmosStore`. The `CosmosStore`'s context is a single collection or part of a collection when using the collection sharing feature. That way, we have an entry point with a single responsibility and authority to operate to only what it needs to know about.

## How do I use Cosmonaut

The idea is pretty simple. You can have one CosmosStore per entity (POCO/dtos etc).
This entity will be used to create a collection or use part of a one in CosmosDB and it will offer all the data access for this object.

Registering the CosmosStores in ServiceCollection for DI support
```c#
 var cosmosSettings = new CosmosStoreSettings("<<databaseName>>", "<<cosmosUri>>", "<<authkey>>");
                
serviceCollection.AddCosmosStore<Book>(cosmosSettings);

//or just by using the Action extension

serviceCollection.AddCosmosStore<Book>("<<databaseName>>", "<<cosmosUri>>", "<<authkey>>", settings =>
{
    settings.ConnectionPolicy = connectionPolicy;
    settings.DefaultCollectionThroughput = 5000;
    settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1),
        new RangeIndex(DataType.String, -1));
});

//or just initialise the object

ICosmosStore<Book> bookStore = new CosmosStore<Book>(cosmosSettings)
```

To use the `AddCosmosStore` extension methods you need to install the `Cosmonaut.Extensions.Microsoft.DependencyInjection` package.

```c#
Install-Package Cosmonaut.Extensions.Microsoft.DependencyInjection
or
dotnet add package Cosmonaut.Extensions.Microsoft.DependencyInjection
```

## Resources

- [How to easily start using CosmosDB in your C# application in no time with Cosmonaut](http://chapsas.com/how-to-easily-start-using-cosmosdb-in-your-c-application-in-no-time-with-cosmonaut/)
- [(Video) Getting started with .NET Core and CosmosDB using Cosmonaut](http://chapsas.com/video-getting-started-with-net-core-and-cosmosdb-using-cosmonaut/)
- [(Video) How to save money in CosmosDB with Cosmonaut's Collection Sharing](http://chapsas.com/video-how-to-save-money-in-cosmosdb-with-cosmonauts-collection-sharing/)
- [CosmosDB Fluent Pagination with Cosmonaut](http://chapsas.com/cosmosdb-fluent-pagination-with-cosmonaut/)
- [Implementing server side CosmosDB pagination in a Blazor Web App (Part 1: Page Number and Page Size)
](https://chapsas.com/implementing-skiptake-server-side-cosmosdb-pagination-in-a-blazor-web-app/)
- [Implementing server side CosmosDB pagination in a Blazor Web App (Part 2: Next/Previous Page)
](https://chapsas.com/implementing-server-side-cosmosdb-pagination-in-a-blazor-web-app-part-2-next-page-previous-page/)
- The `samples` folder in this project
- [Web app server-side pagination for CosmosDB](https://github.com/Elfocrash/CosmosDBPaginationSample)