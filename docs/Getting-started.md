# Getting Started Guide

## What is Cosmonaut?

Cosmonaut is a supercharged Azure CosmosD DB SDK for the SQL API with ORM support. It eliminates the need for most of the data-access code that developers usually need to write and it limits the unit of work scope to the object itself that the developer needs to work with.

## Why use Cosmonaut?

The official Cosmos DB SDK has a ton of features and it can do a lot of things, but there is no clear path when it comes to doing those things. There is object mapping but the scope always stays the same.

Cosmonaut limits the scope from the Database account level to the `CosmosStore`. The `CosmosStore`'s context is a single collection or part of a collection when using the collection sharing feature. That way, we have an entry point with a single responsibility and authority to operate to only what it needs to know about.

## How do I use Cosmonaut