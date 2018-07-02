[![Build status](https://ci.appveyor.com/api/projects/status/au32jna62iue4wut?svg=true)](https://ci.appveyor.com/project/Elfocrash/cosmonaut) [![NuGet Package](https://img.shields.io/nuget/v/Cosmonaut.svg)](https://www.nuget.org/packages/Cosmonaut)

# What is Cosmonaut?

> The word was derived from "kosmos" (Ancient Greek: κόσμος) which means world/universe and "nautes" (Ancient Greek: ναῦς) which means sailor/navigator

Cosmonaut is an object mapper that enables .NET developers to work with a CosmosDB using .NET objects. It eliminates the need for most of the data-access code that developers usually need to write.

### Getting started

- [How to easily start using CosmosDB in your C# application in no time with Cosmonaut](http://chapsas.com/how-to-easily-start-using-cosmosdb-in-your-c-application-in-no-time-with-cosmonaut/)
- [(Video) Getting started with .NET Core and CosmosDB using Cosmonaut](http://chapsas.com/video-getting-started-with-net-core-and-cosmosdb-using-cosmonaut/)
- [(Video) How to save money in CosmosDB with Cosmonaut's Collection Sharing](http://chapsas.com/video-how-to-save-money-in-cosmosdb-with-cosmonauts-collection-sharing/)

### Usage 
The idea is pretty simple. You can have one CosmoStore per entity (POCO/dtos etc)
This entity will be used to create a collection in the cosmosdb and it will offer all the data access for this object

Registering the CosmosStores in ServiceCollection for DI support
```csharp
 var cosmosSettings = new CosmosStoreSettings("<<databaseName>>", 
    "<<cosmosUri>>"), 
    "<<authkey>>");
                
serviceCollection.AddCosmosStore<Book>(cosmosSettings);

//or just by using the Action extension

serviceCollection.AddCosmosStore<Book>(options =>
            {
                options.DatabaseName = "<<databaseName>>";
                options.AuthKey = "<<authkey>>";
                options.EndpointUrl = new Uri("<<cosmosUri>>");
            });

//or just initialise the object

ICosmosStore<Book> bookStore = new CosmosStore<Book>(cosmosSettings)
```

##### Quering for entities

In order to query for entities all you have to do is call the `.Query()` method and then use LINQ to create the query you want.
It is HIGHLY recommended that you use one of the `Async` methods to get the results back, such as `ToListAsync` or `FirstOrDefaultAsync` , when available.

```csharp
var user = await cosmoStore.Query().FirstOrDefaultAsync(x => x.Username == "elfocrash");
var users = await cosmoStore.Query().Where(x => x.HairColor == HairColor.Black).ToListAsync(cancellationToken);

// or you can use SQL

var user = await cosmoStore.QueryMultipleAsync("select * from c w.Firstname = 'Smith'");
```

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

#### Collection sharing
Cosmonaut is all about making the integration with CosmosDB easy as well as making things like cost optimisation part of the library.

That's why Cosmonaut support collection sharing between different types of entities.

Why would you do that?

Cosmos is charging you based on how many RU/s your individual collection is provisioned at. This means that if you don't need to have one collection per entity because you won't use it that much, even on the minimum 400 RU/s, you will be charged money. That's where the magic of schemaless comes in.

How can you do that?

Well it's actually pretty simple. Just implement the `ISharedCosmosEntity` interface and decorate your object with the `SharedCosmosCollection` attribute.

The attribute accepts two properties, `SharedCollectionName` which is mandatory and `EntityPrefix` which is optional.
The `SharedCollectionName` property will be used to name the collection that the entity will share with other entities. 

The `CosmosEntityName` will be used to make the object identifiable for Cosmosnaut. Be default it will pluralize the name of the class, but you can specify it to override this behavior.

Once you set this up you can add individual CosmosStores with shared collections.

Something worths noting is that because you will use this to share objects partitioning will be virtually impossible. For that reason the `id` will be used as a partition key by default as it is the only property that will be definately shared between all objects.


#### Indexing
By default CosmosDB is created with the following indexing rules

```javascript
{
    "indexingMode": "consistent",
    "automatic": true,
    "includedPaths": [
        {
            "path": "/*",
            "indexes": [
                {
                    "kind": "Range",
                    "dataType": "Number",
                    "precision": -1
                },
                {
                    "kind": "Hash",
                    "dataType": "String",
                    "precision": 3
                }
            ]
        }
    ],
    "excludedPaths": []
}
```

Indexing in necessary for things like querying the collections.
Keep in mind that when you manage indexing policy, you can make fine-grained trade-offs between index storage overhead, write and query throughput, and query consistency.

For example if the String datatype is Hash then exact matches like the following,
`cosmoStore.Query().FirstOrDefaultAsync(x => x.SomeProperty.Equals($"Nick Chapsas")`
will return the item if it exists in CosmosDB but 
`cosmoStore.Query().FirstOrDefaultAsync(x => x.SomeProperty.StartsWith($"Nick Ch")`
will throw an error. Changing the Hash to Range will work.

However you can get around that by setting the `FeedOptions.EnableScanInQuery` to `true` for this `Query()`

More about CosmosDB Indexing [here](https://docs.microsoft.com/en-us/azure/cosmos-db/indexing-policies)

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

#### Collection naming
Your collections will automatically be named based on the plural of the object you are using in the generic type.
However you can override that by decorating the class with the `CosmosCollection` attribute.

Example:
```csharp
[CosmosCollection("somename")]
```

### Logging

#### Event source

Cosmonaut uses the .NET Standard's `System.Diagnostics` to log it's actions as dependency events. 
By default, this system is deactivated. In order to activated and actually do something with those events you need to create an  `EventListener` which will activate the logging and give you the option do something with the logs.

#### `Cosmonaut.ApplicationInsights`

By using this package you are able to log the events as dependencies in [Application Insights](https://azure.microsoft.com/en-gb/services/application-insights/) in detail. The logs are batched and send in intervals OR automatically sent when the batch buffer is filled to max.

### Restrictions
Because of the way the internal `id` property of Cosmosdb works, there is a mandatory restriction made.
You cannot have a property named Id or a property with the attribute `[JsonProperty("id")]` without it being a string.
A cosmos id need to exist somehow on your entity model. For that reason if it isn't part of your entity you can just implement the `ICosmosEntity` interface or extend the `CosmosEntity` class.
