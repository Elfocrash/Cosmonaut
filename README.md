[![Build status](https://ci.appveyor.com/api/projects/status/au32jna62iue4wut?svg=true)](https://ci.appveyor.com/project/Elfocrash/cosmonaut) [![NuGet Package](https://img.shields.io/nuget/v/Cosmonaut.svg)](https://www.nuget.org/packages/Cosmonaut)

# What is Cosmonaut?

> The word was derived from "kosmos" (Ancient Greek: κόσμος) which means world/universe and "nautes" (Ancient Greek: ναῦς) which means sailor/navigator

Cosmonaut is an object-relational mapper (O/RM) that enables .NET developers to work with a CosmosDB using .NET objects. It eliminates the need for most of the data-access code that developers usually need to write. Sounds familiar? It's because it's heavily inspired by Entity Framework.

### Usage 
The idea is pretty simple. You can have one CosmoStore per entity (POCO/dtos etc)
This entity will be used to create a collection in the cosmosdb and it will offer all the data access for this object

Registering the CosmosStores in ServiceCollection for DI support
```csharp
 var cosmosSettings = new CosmosStoreSettings("<<databaseName>>", 
    new Uri("<<cosmosUri>>"), 
    "<<authkey>>");
                
serviceCollection.AddCosmosStore<Book>(cosmosSettings);
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

##### Quering for entities
```csharp
var user = await cosmoStore.FirstOrDefaultAsync(x => x.Username == "elfocrash");
var users = await cosmoStore.ToListAsync(x => x.HairColor == HairColor.Black);
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
Update will check if the item exists and it will only update it if it does exist.
Upsert on the other hand will either add the item if there is no item with this id or update it if an item with this id exists.

Upsert is significantly faster than Update as it doesn't check if the item exists before it operates. It is also cheaper when it comes to RUs.

##### Removing entities
```csharp
await cosmoStore.RemoveAsync(x => x.Name == "Nick"); // Removes all the entities that match the criteria
await cosmoStore.RemoveAsync(entity);// Removes the specific entity
await cosmoStore.RemoveByIdAsync("<<anId>>");// Removes an entity with the specified ID
```

#### Performance
Performance can vary dramatically based on the throughput (RU/s*) you are using.
By default Cosmonaut will set the throughput to the lowest value of `400` mainly because I don't want to affect how much you pay accidentaly.
You can set the default throughput for all the collections when you set up your `CosmosStore` by setting the `CollectionThroughput` option to whatever you see fit or by simply setting it in Azure.
You can also set the throughput at the collection level by using the `CosmosCollection` attribute at the entity's class.

Example:
```csharp
[CosmosCollection(Throughput = 1000)]
```

Note here that this functionality is disabled by default. Usage of Azure to adjust is recommended.

#### Collection naming
Your collections will automatically be named based on the plural of the object you are using in the generic type.
However you can override that by decorating the class with the `CosmosCollection` attribute.

Example:
```csharp
[CosmosCollection("somename")]
```

### Restrictions
Because of the way the internal `id` property of Cosmosdb works, there is a mandatory restriction made.
You cannot have a property named Id or a property with the attribute `[JsonProperty("id")]` without it being a string.
A cosmos id need to exist somehow on your entity model. For that reason if it isn't part of your entity you can just implement the `ICosmosEntity` interface.
