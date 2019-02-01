# Collection sharing

## What is collection sharing?

When development on Cosmonaut started there was no option to provision RUs on the database level. Later this feature came in and it has a 50k RUs minimum. It was later reduced to 10k and now it's on 400 RUs for the whole database. That's fine and all but having collection level throughput is still to me the best way to go. It limits the scalability to a single collection.

Collection sharing is the consept of having multiple different types of objects sharing the same collection while Cosmonaut is able to operate on them as if they were in completely different containers.

The benefit of such a feature is that you don't need a single collection per entity type. You can simply have them sharing. If you are also good with your partitioning strategy you will be able to have multiple shared collections with different partition key definitions that make sense and provide optimal read and write performance.

## How can I use Collection sharing?

In order to enable collection sharing you all you have to do is have your POCO  implement the `ISharedCosmosEntity` interface and decorate the class with the `SharedCosmosCollectionAttribute`.

An example of an object that is hosted in a shared collection looks like this:

```c#
[SharedCosmosCollection("shared", "somebooks")]
public class Book : ISharedCosmosEntity
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [CosmosPartitionKey]
    public string Name { get; set; }

    public string CosmosEntityName { get; set; }
}
```

The first parameter at the `SharedCosmosCollection` attribute with value `shared` represents the shared collection name that this object will use. This is the only mandatory parameter for this attribute. The second one with value `somebooks` represents the value that `CosmosEntityName` will automatically be populated with. If you don't set this value then a lowercase pluralised version of the class name will be used instead.

> Note: You do NOT need to set the `CosmosEntityName` value yourself. Leave it as it is and Cosmonaut will do the rest for you.

Even though this is convinient I understand that you might need to have a more dynamic way of specifying the collection that this object should use. That's why the `CosmosStore` class has some extra constructors that allow you to specify the `overriddenCollectionName` property. This property will override any collection name specified at the attribute level and will use that one instead.

> Note: If you have specified a CollectionPrefix at the CosmosStoreSettings level it will still be added. You are only overriding the collection name that the attribute would normally set.

If you want your shared collection to be partitioned then make sure than the partition key definition is the same in all the objects that are hosted in this collection.

You can also use the `SharedCosmosCollection` constructor overload that uses the `UseEntityFullName` boolean. By using that constructor Cosmonaut will automatically assign the full namespace of the entity as the discriminator value.
