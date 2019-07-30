[![Build Status](https://dev.azure.com/nickchapsas/Cosmonaut/_apis/build/status/Elfocrash.Cosmonaut)](https://dev.azure.com/nickchapsas/Cosmonaut/_build/latest?definitionId=2) [![NuGet Package](https://img.shields.io/nuget/v/Cosmonaut.svg)](https://www.nuget.org/packages/Cosmonaut) [![NuGet](https://img.shields.io/nuget/dt/cosmonaut.svg)](https://www.nuget.org/packages/cosmonaut) [![Documentation Status](https://readthedocs.org/projects/cosmonaut/badge/?version=latest)](https://cosmonaut.readthedocs.io/en/latest/?badge=latest) [![Licensed under the MIT License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Elfocrash/Cosmonaut/blob/master/LICENSE)

# Cosmonaut

![](https://raw.githubusercontent.com/Elfocrash/Cosmonaut/develop/logo.png)

> The word was derived from "kosmos" (Ancient Greek: κόσμος) which means world/universe and "nautes" (Ancient Greek: ναῦς) which means sailor/navigator

Cosmonaut is a supercharged SDK with object mapping capabilities that enables .NET developers to work with CosmosDB. It eliminates the need for most of the data-access code that developers usually need to write.

### [Documentation](https://cosmonaut.readthedocs.io/en/latest)

### Getting started

- [How to easily start using CosmosDB in your C# application in no time with Cosmonaut](http://chapsas.com/how-to-easily-start-using-cosmosdb-in-your-c-application-in-no-time-with-cosmonaut/)
- [(Video) Getting started with .NET Core and CosmosDB using Cosmonaut](http://chapsas.com/video-getting-started-with-net-core-and-cosmosdb-using-cosmonaut/)
- [(Video) How to save money in CosmosDB with Cosmonaut's Collection Sharing](http://chapsas.com/video-how-to-save-money-in-cosmosdb-with-cosmonauts-collection-sharing/)
- [CosmosDB Fluent Pagination with Cosmonaut](http://chapsas.com/cosmosdb-fluent-pagination-with-cosmonaut/)
- [Implementing server side CosmosDB pagination in a Blazor Web App (Part 1: Page Number and Page Size)
](https://chapsas.com/implementing-skiptake-server-side-cosmosdb-pagination-in-a-blazor-web-app/)
- [Implementing server side CosmosDB pagination in a Blazor Web App (Part 2: Next/Previous Page)
](https://chapsas.com/implementing-server-side-cosmosdb-pagination-in-a-blazor-web-app-part-2-next-page-previous-page/)

### Samples
 - The `samples` folder in this project
 - [Web app server-side pagination for CosmosDB](https://github.com/Elfocrash/CosmosDBPaginationSample)

### Usage 
The idea is pretty simple. You can have one CosmosStore per entity (POCO/dtos etc).
This entity will be used to create a collection or use part of a one in CosmosDB and it will offer all the data access for this object.

Registering the CosmosStores in ServiceCollection for DI support
```csharp
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

```
Install-Package Cosmonaut.Extensions.Microsoft.DependencyInjection
or
dotnet add package Cosmonaut.Extensions.Microsoft.DependencyInjection
```

##### Retrieving an entity by id (and partition key)

```csharp
var user = await cosmosStore.FindAsync("userId");
var user = await cosmosStore.FindAsync("userId", "partitionKey");
var user = await cosmosStore.FindAsync("userId", new RequestOptions());
```

##### Querying for entities using LINQ

In order to query for entities all you have to do is call the `.Query()` method and then use LINQ to create the query you want.
It is HIGHLY recommended that you use one of the `Async` methods to get the results back, such as `ToListAsync` or `FirstOrDefaultAsync` , when available.

```csharp
var user = await cosmoStore.Query().FirstOrDefaultAsync(x => x.Username == "elfocrash");
var users = await cosmoStore.Query().Where(x => x.HairColor == HairColor.Black).ToListAsync(cancellationToken);
```

##### Querying for entities using SQL

```csharp
// plain sql query
var user = await cosmoStore.Query("select * from c where c.Firstname = 'Smith'").ToListAsync();
or
var user = await cosmoStore.QueryMultipleAsync("select * from c where c.Firstname = 'Smith'");

// or parameterised sql query
var user = await cosmoStore.QueryMultipleAsync("select * from c where c.Firstname = @name", new { name = "Smith" });
```

#### Collection sharing
Cosmonaut is all about making integrating with Cosmos DB easy as well as making things such as cost optimisation part of the library.

That's why Cosmonaut supports transparent collection sharing between different types of entities.

Why would you do that?

Cosmos is charging you based on how many RU/s your individual collection is provisioned at. This means that if you don't need to have one collection per entity because you won't use it that much, even on the minimum 400 RU/s, you will be charged money. That's where the magic of schemaless comes in.

How can you do that?

Well it's actually pretty simple. Just implement the `ISharedCosmosEntity` interface and decorate your object with the `SharedCosmosCollection` attribute.

The attribute accepts two properties, `SharedCollectionName` which is mandatory and `EntityName` which is optional.
The `SharedCollectionName` property will be used to name the collection that the entity will share with other entities. 

The `EntityName` will be used to make the object identifiable for Cosmosnaut. By default it will pluralize the name of the class, but you can specify it to override this behavior. You can override this by providing your own name by setting the `EntityName` value at the attribute level.

Once you set this up you can add individual CosmosStores with shared collections.


#### Collection naming

Your collections will automatically be named based on the plural of the object you are using in the generic type.
However you can override that by decorating the class with the `CosmosCollection` attribute.

Example:
```csharp
[CosmosCollection("somename")]
```

By default you are required to specify your collection name in the attribute level shared entities like this:

```c#
[SharedCosmosCollection("shared")]
public class Car : ISharedCosmosEntity
{
    public string Id { get; set; }
    public string CosmosEntityName { get; set; }
}
```

Even though this is convenient I understand that you might need to have a dynamic way of setting this. 
That's why the `CosmosStore` class has some extra constructors that allow you to specify the `overriddenCollectionName` property. This property will override any collection name specified at the attribute level and will use that one instead.

Note: If you have specified a `CollectionPrefix` at the `CosmosStoreSettings` level it will still be added. You are only overriding the collection name that the attribute would normally set.

Example

Class implementation:
```c#
[SharedCosmosCollection("shared")]
public class Car : ISharedCosmosEntity
{
    public string Id { get; set; }
    public string CosmosEntityName { get; set; }
}
```

CosmosStore initialisation:
```c#
var cosmosStore = new CosmosStore<Car>(someSettings, "oldcars");
```

The outcome of this would be a collection named `oldcars` because the `shared` collection name is overridden in the constructor. 
There are also method overloads for the same property at the dependency injection extension level.

#### Pagination

Cosmonaut supports two types of pagination.

* Page number + Page size
* ContinuationToken + Page size

Both of these methods work by adding the `.WithPagination()` method after you used any of the `Query` methods.

```csharp
var firstPage = await booksStore.Query().WithPagination(1, 10).OrderBy(x=>x.Name).ToListAsync();
var secondPage = await booksStore.Query().WithPagination(2, 10).OrderBy(x => x.Name).ToPagedListAsync();
var thirdPage = await booksStore.Query().WithPagination(secondPage.NextPageToken, 10).OrderBy(x => x.Name).ToPagedListAsync();
var fourthPage = await thirdPage.GetNextPageAsync();
var fifthPage = await booksStore.Query().WithPagination(5, 10).OrderBy(x => x.Name).ToListAsync();
```

`ToListAsync()` on a paged query will just return the results. `ToPagedListAsync()` on the other hand will return a `CosmosPagedResults` object. This object contains the results but also a boolean indicating whether there are more pages after the one you just got but also the continuation token you need to use to get the next page.

##### Pagination recommendations

Because page number + page size pagination goes though all the documents until it gets to the requested page, it's potentially slow and expensive.
The recommended approach would be to use the page number + page size approach once for the first page and get the results using the `.ToPagedListAsync()` method. This method will return the next continuation token and it will also tell you if there are more pages for this query. Then use the continuation token alternative of `WithPagination` to continue from your last query.

Keep in mind that this approach means that you have to keep state on the client for the next query, but that's what you'd do if you where using previous/next buttons anyway.

##### Adding an entity in the entity store
```csharp
var newUser = new User
{
    Name = "Nick"
};
var added = await cosmoStore.AddAsync(newUser);

var multiple = await cosmoStore.AddRangeAsync(manyManyUsers);
```

##### Updating entities
When it comes to updating you have two options.

Update...
```csharp
await cosmoStore.UpdateAsync(entity);
```

... and Upsert
```csharp
await cosmoStore.UpsertAsync(entity);
```

The main difference is of course in the functionality.
Update will only update if the item you are updating exists in the database with this id.
Upsert on the other hand will either add the item if there is no item with this id or update it if an item with this id exists.

##### Removing entities
```csharp
await cosmoStore.RemoveAsync(x => x.Name == "Nick"); // Removes all the entities that match the criteria
await cosmoStore.RemoveAsync(entity);// Removes the specific entity
await cosmoStore.RemoveByIdAsync("<<anId>>");// Removes an entity with the specified ID
```

#### Response Handling
Cosmonaut follows a different approach when it comes to error handling. The CosmosDB SDK is throwing exceptions for almost every type of error. Cosmonaut follows a different approach.

In Cosmonaut methods that return `CosmosResponse` or `CosmosMultipleResponse` won't throw exceptions for the following errors: `ResourceNotFound`, `PreconditionFailed` and `Conflict`. 
They will instead return a `CosmosResponse` with the `IsSuccess` flag to `false`, the `CosmosOperationStatus` enum explaining what the error was and the `Exception` object containing the exceptions that caused the request to fail.

On top of that, any methods that return `ResourceResponse<T>` in the CosmonautClient will not throw an exception for `ResourceNotFound` and they will instead return `null`.

#### Restrictions
Because of the way the internal `id` property of Cosmosdb works, there is a mandatory restriction made.
You cannot have a property named Id or a property with the attribute `[JsonProperty("id")]` without it being a string.
A cosmos id needs to exist somehow on your entity model. For that reason if it isn't part of your entity you can just extend the `CosmosEntity` class.

It is **HIGHLY RECOMMENDED** that you decorate your Id property with the `[JsonProperty("id")]` attribute to prevent any unexpected behaviour.

#### CosmonautClient

Cosmonaut has its own version of a `DocumentClient` called `CosmonautClient`. The difference is that the `CosmonautClient` interface is more user friendly and it looks more like something you would use in a real life scenario. It won't throw not found exceptions if an item is not found but it will return `null` instead. It will also retry automatically when you get 429s (too many requests).

It also has support for logging and monitoring as you are going to see in the logging section of this page.

#### Transactions

There is currently no way to reliably do transactions with the current CosmosDB SDK. Because Cosmonaut is a wrapper around the CosmosDB SDK it doesn't support them either. However there are plans for investigating potential other ways to achieve transactional operations such as server side stored procedures that Cosmonaut could provision and call.

Every operational call (Add, Update, Upsert, Delete) however returns it's status back alongside the reason it failed, if it failed, and the entity so you can add your own retry logic.

#### Partitioning
Cosmonaut supports partitions out of the box. You can specify which property you want to be your Partition Key by adding the `[CosmosPartitionKey]` attribute above it.

Unless you really know what you're doing, it is recommended make your `Id` property the Partition Key. This will enable random distribution for your collection.

If you do not set a Partition Key then the collection created will be single partition. Here is a quote from Microsoft about single partition collections: 
> Single-partition collections have lower price options and the ability to execute queries and perform transactions across all collection data. They have the scalability and storage limits of a single partition (10GB and 10,000 RU/s). You do not have to specify a partition key for these collections. For scenarios that do not need large volumes of storage or throughput, single partition collections are a good fit.
[link](https://azure.microsoft.com/en-gb/blog/10-things-to-know-about-documentdb-partitioned-collections/)

##### Known hiccups
Partitions are great but you should these 3 very important things about them and about the way Cosmonaut will react.

* Once a collection is created with a partition key, it cannot be removed or changed.
* You cannot add a partition key later to a single partition collection.
* If you use the the Upsert method to update an entity that had the value of the property that is the partition key changed, then CosmosDB won't update the document but instead it will create a whole different document with the same id but the changed partition key value.

More on the third issue here [Unique keys in Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/unique-keys)

#### Optimizing for performance

Cosmonaut by default will create one `CosmonautClient` (which is really a wrapper around the `DocumentClient`) per `CosmosStore`. The logic behind that decision was that each `CosmosStore` might have different configuration from another even on the client level. However in scenarios where you have tens of CosmosStores this can cause socket starvation. The recommendation in such scenarios is to either reuse the same `CosmonautClient` or to cache the CosmosStores internally and swap them around for different CosmosStores. You can see this issue where a multi tenant scenario is discussed and resolved by the use of a client cache.

It is also a good idea in general to create a `CosmonautClient` outside of the CosmosStore logic and  reuse the `CosmonautClient` instead of creating one each time if the configuration for the client is the same.

### Logging

#### Event source

Cosmonaut uses the .NET Standard's `System.Diagnostics` to log it's actions as dependency events. 
By default, this system is deactivated. In order to activated and actually do something with those events you need to create an  `EventListener` which will activate the logging and give you the option do something with the logs.

#### `Cosmonaut.ApplicationInsights`

By using this package you are able to log the events as dependencies in [Application Insights](https://azure.microsoft.com/en-gb/services/application-insights/) in detail. The logs are batched and send in intervals OR automatically sent when the batch buffer is filled to max.

Just initialise the AppInsightsTelemetryModule in your Startup or setup pipeline like this.
Example: `AppInsightsTelemetryModule.Instance.Initialize(new TelemetryConfiguration("InstrumentationKey"))`

If you already have initialised `TelemetryConfiguration` for your application then use `TelemetryConfiguration.Active` instead of `new TelemetryConfiguration` because if you don't there will be no association between the dependency calls and the parent request.
