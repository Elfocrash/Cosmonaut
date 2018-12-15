# Preparing your classes for the CosmosStore

There are some things you need to get familiar with when it comes to using Cosmonaut. You see, Cosmonaut it doing a lot of things for you behind the scenes but it's performing even better if you do just a little bit of configuration.

## Key attributes

### [CosmosCollection]

The `CosmosCollection` attribute is an optional attribute that you can decorate your entity's class with. It has two purposes. 

First it allows you to override the default Cosmonaut collection naming bahaviour, which is to name your collections as a lowercase plularised version of the class name.

Second it allows you to map your class to a pre existing collection with a different name.

> You can of course do a further override of the `CosmosStore` target name at the `CosmosStore` constructor level by providing the `overriddenCollectionName` parameter.

### [SharedCosmosCollection]

In order to enable collection sharing you all you have to do is have your POCO  implement the `ISharedCosmosEntity` interface and decorate the class with the `SharedCosmosCollectionAttribute`.

This attribute has one mandatory and one optional parameter namely the `SharedCollectionName` and the `EntityName`.

```c#
[SharedCosmosCollection("shared", "somebooks")]
```

The first parameter at the `SharedCosmosCollection` attribute with value `shared` represents the shared collection name that this object will use. This is the only mandatory parameter for this attribute. The second one with value `somebooks` represents the value that `CosmosEntityName` will automatically be populated with. If you don't set this value then a lowercase pluralised version of the class name will be used instead.

> You can of course do a further override of the `CosmosStore` target name at the `CosmosStore` constructor level by providing the `overriddenCollectionName` parameter.

### [CosmosPartitionKey]

This is a parameterless attribute. It is used to decorate the property that represents the partition key definition of your collection.

```c#
public class Book
{
    [CosmosPartitionKey]
    public string Name { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
}
```

In this case I have a partition key definition in my collection named `/Name`.

By decorating the `Name` property with the `CosmosPartitionKey` attribute I enable Cosmonaut to do a lot of behind the scenes operation optimisation where it will set the partition key value for you if present, speeding up the operation and making it more cost efficient.

### [JsonProperty("id")]

This is not a Cosmonaut attribute and it is coming from JSON.NET which the underlying Cosmos DB SDK is using.

Even though not required I strongly recommend that you decorate your property which represents the id of the entity with the `[JsonProperty("id")]` attribute. This will prevent any unwanted behaviour with LINQ to SQL conversions and the id property not being mapped propertly.