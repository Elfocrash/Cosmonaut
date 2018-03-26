# Cosmonaut
A simple and easy to use entity store for Microsoft's CosmosDB. (Which is in work in progress atm so don't use it)

# Summary
I really like CosmosDB. However working with it is not as straightforward as you'd expect it to be. 
The usage I'm personally making is to store data in CosmosDB the same way I would store data in Entity Framework.
However even basic CRUD operations with Microsoft's SDK is not as simple and easy for everyone.

That's where Cosmonaut come's in the picture. It's meant to be what the DbContext is for Entity Framework.
A simple and easy way to basic CRUD without missing out on the depth that Microsoft's low level SDK is offering.

### Usage (Take this with a grain of salt as it's still WIP)
The idea is pretty simple. You can have one CosmoStore per entity (POCO/dtos etc)
This entity will be used to create a collection in the cosmosdb and it will offer CRUD+ operations for this object

Example
```csharp
var newUser = new TestUser
{
    Id = Guid.NewGuid().ToString(),
    Name = "Nick"
};
var documentClient = new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
var cosmoStore = new CosmoStore<TestUser>(documentClient, databaseName);
var added = await cosmoStore.AddAsync(newUser);
```

You can also retrieve data based on any property just like you would with EF
```csharp
var user = await cosmoStore.FirstOrDefaultAsync(x => x.Id == "Nick");
```

### Restrictions
Because of the way the internal `id` property of Cosmosdb works, there is a mandatory restriction made.
You cannot have a property named Id or a property with the attribute `[JsonProperty("id")]` without it being a string.
The id property isn't mandatory, however if you need an id it needs to be a string.
