# The CosmosStore

### What is it and why do I care?

The main data context you will be working with while using Cosmonaut is the CosmosStore. The CosmosStore requires you to provide the entity model that it will be working with. 

For example if I only wanted to work with the class `Book` my CosmosStore initialisation would look like this:

```c#
ICosmosStore<Book> bookStore = new CosmosStore<Book>(cosmosSettings)
```

> But what is the context of the CosmosStore? What will I get if I query for all the items in a CosmosStore?

The CosmosStore's boundaries can be one of two. 

* One entity is stored in it's own collection (ie books)
* One entity is stored in a shared collection that other entities live as well (ie library)

The choice to go with one or the other is completely up to you and it comes down to partitioning strategy, cost and flexibility when it comes to scaleability.

### A single mandatory property

In order for an entity to be able to be stored, manipulated and retrieved in a CosmosStore it is required to have a mandatory property. This is the `id` property.

It needs to be a `string` property with the name either being `Id` (with any capitalisation) or any other name but decorated with the `[JsonProperty("id")]` attribute. Even though not neccessary, when the property is named Id, you should also decorate it with the `[JsonProperty("id")]` attribute. This is neccessary if you want to do any querying based on the id (querying NOT reading). It will also help with unintented behavour when it comes to object mapping and LINQ to SQL transformations.

### CosmosStore's lifetime

CosmosStores should be registered as *singletons* in your system. This will achieve optimal performance. If you are using a dependency injection framework make sure they are registered as singletons and if you don't, just make sure you don't dispose them and you keep reusing them.

### CosmosStoreSettings

The CosmosStore can be initialised in multiple ways but the recommended one is by providing the `CosmosStoreSettings` object.

The `CosmosStoreSettings` object can be initialised requires 3 parameters in order to be created. The database name, the Cosmos DB endpoint Uri and the auth key.

```c#
 var cosmosSettings = new CosmosStoreSettings("<<databaseName>>", "<<cosmosUri>>", "<<authkey>>");
```

There are other optional settings you can provide such as:

* `ConnectionPolicy` - The connection policy for this CosmosStore.
* `ConsistencyLevel` - The level of consistency for this CosmosStore.
* `IndexingPolicy` - The indexing policy for this CosmosStore if it's collection in not yet created.
* `DefaultDatabaseThroughput` - The default database level throughput. No database throughput by default.
* `OnDatabaseThroughput` - The action to be taken when the collection that is about to be created is part of a database that has RU/s provisioned for it. `UseDatabaseThroughput` will ignore the `DefaultCollectionThroughput` and use the database's RUs. `DedicateCollectionThroughput` will provision dedicated RUs for the collection of top of the database throughput with the value of `DefaultCollectionThroughput`.
* `DefaultCollectionThroughput` - The default throughput for this CosmosStore if it's collection in not yet created.
* `JsonSerializerSettings` - The object to json serialization settings.
* `InfiniteRetries` - Whether you want infinite retries on throttled requests.
* `CollectionPrefix` - A prefix prepended on the collection name.
* `ProvisionInfrastructureIfMissing` - Whether the `CosmosStore` will automatically provision the infrastructure when the `CosmosStore` is instantiated. Default `true`.

> Note: In some scenarios, especially with .NET Framework apps, you might notice that inintialisation of the `CosmosStore` can cause a deadlock. This is due to it's call from the UI thread and a synchronisation context issue. To work around that, you can simply set the `ProvisionInfrastructureIfMissing` to `false` and then use the `CosmosStore`'s `EnsureInfrastructureProvisionedAsync` method awaited properly. 

### CosmosResponse and response handling

By default, Cosmos DB throws exceptions for any bad operation. This includes reading documents that don't exist, pre condition failures or trying to add a document that already exists.

This makes response handing really painful so Cosmonaut changes that.

Instead of throwing an excpetion Cosmonaut wraps the responses into it's own response called `CosmosResponse`.

This object contains the following properties:

* IsSuccess - Indicates whether the operation was successful or not
* CosmosOperationStatus - A Cosmonaut enum which indicates what the status of the response is
* ResourceResponse - The ResourceResponse<Document> that contains things like RU charge, Etag, headers and all the other info that the response would normally have
* Entity - The object you used for this operation
* Exception - The exception that caused this response to fail

It also has an implicit operation which, if present, will return the entity itself.

#### CosmosOperationStatus

The CosmosOperationStatus operation status can have one of 5 values.

* Success - The operation was successful
* RequestRateIsLarge - Your CosmosDB is under heavy load and it can't handle the request
* ResourceNotFound - The item you tried to update/upsert was not found
* PreconditionFailed - The Etag that you provided is different from the document one in the database indicating that it has been changed
* Conflict - You are trying to add an item that already exists

### Notes

The CosmosStore also exposes the underlying CosmonautClient that it's using to perform operations so you can use that for any other operations you want to make against Cosmos DB. You need to know though that the CosmosStore's context is only limited for it's own methods. Once you use the CosmonautClient or the DocumentClient you are outside of the limitations of CosmosStore so be careful.
