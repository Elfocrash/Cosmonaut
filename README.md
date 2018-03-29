[![Build status](https://ci.appveyor.com/api/projects/status/au32jna62iue4wut?svg=true)](https://ci.appveyor.com/project/Elfocrash/cosmonaut)

# Cosmonaut
A simple and easy to use entity store for Microsoft's CosmosDB. (Which is in work in progress atm so don't use it)

# Summary
I really like CosmosDB. However working with it is not as straightforward as you'd expect it to be. 
The usage I'm personally making is to store data in CosmosDB the same way I would store data in Entity Framework.
However even basic CRUD operations with Microsoft's SDK is not as simple and easy for everyone.

That's where Cosmonaut come's in the picture. It's meant to be what the DbContext is for Entity Framework.
A simple and easy way to basic CRUD without missing out on the depth that Microsoft's low level SDK is offering.

### Usage 
The idea is pretty simple. You can have one CosmoStore per entity (POCO/dtos etc)
This entity will be used to create a collection in the cosmosdb and it will offer CRUD+ operations for this object

Registering the CosmosStores in ServiceCollection for DI support
```csharp
 var cosmosSettings = new CosmosStoreSettings("<<databaseName>>", 
    new Uri("<<cosmosUri>>"), 
    "<<authkey>>");
                
serviceCollection.AddCosmosStore<Book>(cosmosSettings);
```

Adding an entity in the entity store
```csharp
var newUser = new User
{
    Name = "Nick"
};
var added = await cosmoStore.AddAsync(newUser);

var multiple = await cosmoStore.AddRangeAsync(manyManyUsers);
```

Quering for entities
```csharp
var user = await cosmoStore.FirstOrDefaultAsync(x => x.Username == "elfocrash");
var users = await cosmoStore.ToListAsync(x => x.HairColor == HairColor.Black);
```

Updating entities
```csharp
await cosmoStore.UpdateAsync(entity);
```

Removing entities
```csharp
await cosmoStore.RemoveAsync(x => x.Name == "Nick"); // Removes all the entities that match the criteria
await cosmoStore.RemoveAsync(entity);// Removes the specific entity
await cosmoStore.RemoveByIdAsync("<<anId>>");// Removes an entity with the specified ID
```

#### Performance
Performance can vary dramatically based on the throughput (RU/s*) you are using.
By default Cosmonaut will set the throughput to the lowest value of `400` mainly because I don't want to affect how much you pay accidentaly.
You can set the default throughput for all the collections when you set up your `CosmosStore` by setting the `CollectionThroughput` option to whatever you see fit.
You can also set the throughput at the collection level by using the `CosmosCollection` attribute at the entity's class.

Example:
```csharp
[CosmosCollection(Throughput = 1000)]
```

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
