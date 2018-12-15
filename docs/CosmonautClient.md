# The CosmonautClient

### What is it and why do I care?

The CosmonautClient is a wrapper around the DocumentClient that comes from the CosmosDB SDK. It's main purpose is to abstract some of the things you really don't need to know about when it comes to using Cosmos DB. An example would be the UriFactory class.

Normally the DocumentClient call requires you to provide a Uri to resources in order to perform operations. You shouldn't care. All you need to care about is that this call needs a `databaseId` and a `collectionId`. This client wrapper does that.

It also wraps the calls to Cosmos and it profiles them in order to provide performance metrics. This will only happen when you have an active event source. You can learn more about this in the Logging section.

Something worth noting is that the CosmonautClient won't throw an exception for not found documents on methods that the response is `ResourceResponse` but instead it will return null in order to make response handling easier.

Any method that returns a `CosmosResponse` will still obay all the rules described on the "CosmosResponse and response handling" section of the "The CosmosStore" page.