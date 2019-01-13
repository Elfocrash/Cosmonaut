# Pagination

Cosmonaut supports two types of pagination.

* Page number + Page size
* ContinuationToken + Page size

Both of there methods work by adding the `.WithPagination()` method after you used any of the `Query` methods.

```csharp
var firstPage = await booksStore.Query().WithPagination(1, 10).OrderBy(x=>x.Name).ToListAsync();
var secondPage = await booksStore.Query().WithPagination(2, 10).OrderBy(x => x.Name).ToPagedListAsync();
var thirdPage = await booksStore.Query().WithPagination(secondPage.NextPageToken, 10).OrderBy(x => x.Name).ToPagedListAsync();
var fourthPage = await thirdPage.GetNextPageAsync();
var fifthPage = await booksStore.Query().WithPagination(5, 10).OrderBy(x => x.Name).ToListAsync();
```

`ToListAsync()` on a paged query will just return the results. `ToPagedListAsync()` on the other hand will return a `CosmosPagedResults` object. This object contains the results but also a boolean indicating whether there are more pages after the one you just got but also the continuation token you need to use to get the next page.

## Pagination recommendations

Because page number + page size pagination goes though all the documents until it gets to the requested page, it's potentially slow and expensive.
The recommended approach would be to use the page number + page size approach once for the first page and get the results using the `.ToPagedListAsync()` method. This method will return the next continuation token and it will also tell you if there are more pages for this query. Then use the continuation token alternative of `WithPagination` to continue from your last query.

Keep in mind that this approach means that you have to keep state on the client for the next query, but that's what you'd do if you where using previous/next buttons anyway.
