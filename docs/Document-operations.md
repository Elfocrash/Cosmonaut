# Working with Entities using Cosmonaut


## Adding an entity in the CosmosStore
```csharp
var newUser = new User
{
    Name = "Nick"
};
var oneAdded = await cosmoStore.AddAsync(newUser);

var multipleAdded = await cosmoStore.AddRangeAsync(manyManyUsers);
```

Using the Range operation allows you to also provide a `Func` that creates a `RequestOptions` for every individual execution that takes place in the operation

## Updating and Upserting entities
When it comes to updating you have two options.

Update example
```c#
var response = await cosmoStore.UpdateAsync(entity);
```

Upsert example
```c#
var response = await cosmoStore.UpsertAsync(entity);
```

The main difference is of course in the functionality.
Update will only update if the item you are updating exists in the database with this id.
Upsert on the other hand will either add the item if there is no item with this id or update it if an item with this id exists.

There are also `Range` variation of both of these methods.

Using one of the Range operations allows you to also provide a `Func` that creates a `RequestOptions` for every individual execution that takes place in the operation. This might be something the Etag in order to ensure that you are updatng the latest version of the document.

Example of an `UpdateRangeAsync` execution that ensures that the latest version of the document is being updated:

```c#
var updated = await booksStore.UpdateRangeAsync(objectsToUpdate, x => new RequestOptions { AccessCondition = new AccessCondition
{
    Type = AccessConditionType.IfMatch,
    Condition = x.Etag
}});
```

## Removing entities

There are multiple ways to remove an entity in Cosmonaut.

The simplest one is to use any of the overloads of the `RemoveByIdAsync` methods.

```c#
var removedWithId = await cosmoStore.RemoveByIdAsync("documentId");
var removedWithIdAndPartitionKey = await cosmoStore.RemoveByIdAsync("documentId", "partitionKeyValue");
```

There is also the `RemoveAsync` method which uses an entity object to do the removal. However this object needs to have the id property populated and if it's a partitioned, it should also have the partition key value populated.

```c#
var removedEntity = await cosmosStore.RemoveAsync(entity);
```

Last but not least you can use the `RemoveAsync` method that has a predicate in it's signature. This will match all the documents that satistfy the predicate and remove them. You have to keep in mind that this method is doing a cross partition query behind the scenes before it does a direct delete per document. It's not very efficient and it should be used only in rare cases.

```c#
var deleted = await cosmoStore.RemoveAsync(x => x.Name == "Nick");
```

You can specify the `FeedOptions` for the query that takes place, potentially providing a partition key value to limit the scope of the request.

You to also provide a `Func` that creates a `RequestOptions` for every individual execution that takes place in the operation.