# Reading and querying entities

Cosmonaut provides a set of easy to use methods to read and query entities for both CosmosStore and CosmonautClient.

## CosmosStore

### Reading an entity

In order to perform a direct read against a container you can to use the `FindAsync` method. This method has three overloads.

* `FindAsync(documentId, RequestOptions?, CancellationToken?)`
* `FindAsync(documentId, partitionKeyValue, CancellationToken?)`

The first one will use the document id to read the entity. It will *automatically* add the partition key value in the request options if `CosmosPartitionKey` attribute is decorating the id property of the object. In other words, if your id is your partition key and it's decorated with `CosmosPartitionKey` attribute Cosmonaut will automatically set it for you.

The second one is a shorthand of the first one in the sense that if you only need to specify the partition key value without any other options you can use that.

### Querying for entities

Cosmonaut supports two different types of querying when it comes to querying for entities.

* Fluent Querying with LINQ
* Using CosmosDB flavoured SQL

You can enter both modes using the `Query` method of the CosmosStore. There are 2 `Query` methods.

* `Query(FeedOptions?)` - Entry point for LINQ querying
* `Query(string sql, object parameters = null, FeedOptions?, CancellationToken?)` - Entry point for SQL querying

They both return an IQueryable which behind the scenes is a IDocumentQuery. However, if you try to add a `Where` clause or an `OrderBy` on the SQL variation of this method you will get an error. All your querying logic HAS to be in the sql text and it cannot be extended with LINQ after that. You can still use the `.WithPagination` method which we will talk about in the "Pagination" section of the docs.

#### Querying with LINQ

Returning all the items in the CosmosStore.

```c#
var books = await booksStore.Query().ToListAsync();
```

This method will execute a cross partition query without any pagination. This is generally not the recommended approach as it might take a long time to get all the results back. You can instead provide a specific partition key value and get all the books inside a logical partition.

```c#
var books = await booksStore.Query(new FeedOptions{PartitionKey = new PartitionKey("Stephen King")}).ToListAsync();
```

This query will perform way better as we are specifying a specific partition that we want to query.

You can also add a filter and ordering. Let's see how we can return all the books written in 1998 ordered by name.

```c#
var books = await booksStore.Query().Where(x => x.PublishedDate == 1998).OrderBy(x => x.Name).ToListAsync();
```

Simple as that. However you have to keep in mind that this query is again a cross partition query that also does ordering. This will be an inefficient and long running query. Ideally you want to provide the partition key value every time you query for items.

#### Querying with SQL

> The CosmosStore context is always limited to this entity. This means that even if you do `select * from c` from the CosmosStore you won't actually select everything from the collection but everything from the collection for that specific object.

The equivelant of the first LINQ example can be written in SQL like this:

```c#
var books = await booksStore.Query("select * from c").ToListAsync();
var books = await booksStore.QueryMultipleAsync("select * from c");
```

Both methods also accept the `FeedOptions` object that offers options for the query execution and cancellation tokens.

The reason why there are two methods that seem to be doing the same thing is because the first one can also use the `WithPagination` extension after that in order to be converted to a SQL query that also uses pagination. More about pagination can be found on the "Pagination" section of the docs.

Cosmonaut also supports parameterised queries in the same way that Dapper does. You can add the `@` symbol in the filter part of your sql query and then provide an object that contains a property that matches the name of the `@` prefixed paramater in order to validate and replace.

Example:

```c#
var user = await cosmoStore.Query("select * from c where c.LastName = @name", new { name = "Smith" }).ToListAsync();

var user = await cosmoStore.QueryMultipleAsync("select * from c where c.LastName = @name", new { name = "Smith" });
```

**QuerySingleAsync and QueryMultipleAsync**

As we saw above QueryMultipleAsync is a method that returns an IEnumerable of objects based on the query provided. There is also a second overload that accepts a generic T type. The purpose of this method is to allow you to map your SQL result to a different object. The main reason behind this features is that Cosmos DB SQL allows you to select only a few properties to return, so instead of returning a huge document you return only the properties you need.

```c#
var listOfFullNames = await cosmoStore.QueryMultipleAsync<FullName>("select c.FirstName, c.LastName from c");
```

Of course you can achieve the same with LINQ by doing the following.

```c#
var listOfFullNames = await cosmoStore.Query().Select(x => new FullName{ FirstName = x.FirstName; LastName = x.LastName }).ToListAsync();
```

The QuerySingleAsync methods are similar to the Multiple ones with the only difference being that they return a single value. If there are more than 1 value that this query returns then you will get an exception so you'll need to add a `select top 1` if you only need one item to be returned. If nothing is muched it retuns null.

> `ToListAsync` is only one of the asynchronous extension methods that Cosmonaut provides. Check the "Extensions" section of this page for a complete list of extension methods that you can use to query and retrieve data.

## CosmonautClient

The CosmonautClient also offers a few new methods in order to make querying and reading easier.

### Querying for documents/entities

* `Query<T>(databaseId, collectionId, FeedOptions?)` - Fluent LINQ querying entry point similar to the CosmosStore one but without the context limitations.
* `Query<T>(databaseId, collectionId, string sql, object parameters = null, FeedOptions?)` - Fluent SQL querying entry point similar to the CosmosStore one but without the context limitations.
* `QueryDocumentsAsync<T>(string databaseId, string collectionId,
            Expression<Func<Document, bool>> predicate = null, FeedOptions?, CancellationToken?)` - Queries all the documents that match the predicate providies. If the predicate is null then it queries all the documents in the collection. Returns results mapped to generic type `T`.
* `QueryDocumentsAsync(string databaseId, string collectionId,
            Expression<Func<Document, bool>> predicate = null, FeedOptions?, CancellationToken?)` - Queries all the documents that match the predicate provides. If the predicate is null then it queries all the documents in the collection. Returns `Document` results.
* `QueryDocumentsAsync<T>(databaseId, collectionId,
            string sql, object parameters = null, FeedOptions?, CancellationToken?)` - Similar to QueryMultipleAsync<T>

## Result Extensions

Cosmonaut has a set of extension methods that can be used in both LINQ and SQL based IQueryables in order to asynchronously query the containers.


* `ToListAsync<T>` - Asynchronously queries Cosmos DB and returns a list of all the data matching the query.
* `ToPagedListAsync<T>` - Asynchronously queries Cosmos DB and returns a `CosmosPagedResults` response containing a list of all the data matching the query and also whether there are more pages after this query and a continuation token for the next page.
* `FirstAsync<T>` - Asynchronously queries Cosmos DB and returns the first item matching this query. Throws exception if no items are matched.
* `FirstOrDefaultAsync<T>` - Asynchronously queries Cosmos DB and retuns the first item matching this query. Returns null if no items are matched.
* `SingleAsync<T>` - Asynchronously queries Cosmos DB and returns a single item matching this query. Throws exception if no items or more than one item are matched.
* `SingleOrDefaultAsync<T>` - Asynchronously queries Cosmos DB and returns a single item matching this query. Throws exception if more than one item are matched and returns null if no items are matched.
* `CountAsync<T>` - Asynchronously queries Cosmos DB and retuns the count of items matching this query.
* `MaxAsync<T>` - Asynchronously queries Cosmos DB and retuns the maximum value of item matched.
* `MinAsync<T>` - Asynchronously queries Cosmos DB and retuns the minimum value of item matched.
