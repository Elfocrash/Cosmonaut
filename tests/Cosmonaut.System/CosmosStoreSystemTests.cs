using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Cosmonaut.System.Models;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cosmonaut.System
{
    public class CosmosStoreSystemTests : IDisposable
    {
        private readonly ICosmonautClient _cosmonautClient;
        private readonly Uri _emulatorUri = new Uri("https://localhost:8081");
        private readonly string _databaseId = "systemtests";
        private readonly string _collectionName = "testcol";
        private readonly string _emulatorKey =
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly IServiceProvider _serviceProvider;

        private readonly ConnectionPolicy _connectionPolicy = new ConnectionPolicy
        {
            ConnectionProtocol = Protocol.Tcp,
            ConnectionMode = ConnectionMode.Direct
        };

        public CosmosStoreSystemTests()
        {
            _cosmonautClient = new CosmonautClient(_emulatorUri, _emulatorKey, _connectionPolicy);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCosmosStore<Cat>(_databaseId, _emulatorUri, _emulatorKey, settings =>
            {
                settings.ConnectionPolicy = _connectionPolicy;
            }, _collectionName);

            serviceCollection.AddCosmosStore<Dog>(_databaseId, _emulatorUri, _emulatorKey, settings =>
            {
                settings.ConnectionPolicy = _connectionPolicy;
            })
            .AddCosmosStore<Crocodile>(_databaseId, _emulatorUri, _emulatorKey, settings =>
            {
                settings.ConnectionPolicy = _connectionPolicy;
            })
            .AddCosmosStore<Lion>(_databaseId, _emulatorUri, _emulatorKey, settings =>
            {
                settings.ConnectionPolicy = _connectionPolicy;
            });

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task WhenCosmosStoreInitialised_ThenDatabaseAndCollectionIsCreated()
        {
            _serviceProvider.GetService<ICosmosStore<Cat>>();

            var database = await _cosmonautClient.GetDatabaseAsync(_databaseId);
            var collection = await _cosmonautClient.GetCollectionAsync(_databaseId, _collectionName);

            database.Should().NotBeNull();
            database.Id.Should().Be(_databaseId);
            collection.Should().NotBeNull();
            collection.Id.Should().Be(_collectionName);
        }

        [Fact]
        public async Task WhenValidEntitiesAreAdded_ThenAddedResultsAreSuccessful()
        {
            //TODO Clean this up in the future
            var cats = new List<Cat>();
            var dogs = new List<Dog>();
            var crocodiles = new List<Crocodile>();
            var lions = new List<Lion>();
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var crocodileStore = _serviceProvider.GetService<ICosmosStore<Crocodile>>();

            for (var i = 0; i < 50; i++)
            {
                cats.Add(new Cat { Name = Guid.NewGuid().ToString() });
                dogs.Add(new Dog { Name = Guid.NewGuid().ToString() });
                crocodiles.Add(new Crocodile { Name = Guid.NewGuid().ToString() });
                lions.Add(new Lion { Name = Guid.NewGuid().ToString() });
            }

            var addedCats = await catStore.AddRangeAsync(cats);
            var addedDogs = await dogStore.AddRangeAsync(dogs);
            var addeLions = await lionStore.AddRangeAsync(lions);
            var addedCrocodiles = await crocodileStore.AddRangeAsync(crocodiles);

            addedCats.Exception.Should().BeNull();
            addedCats.SuccessfulEntities.Count.Should().Be(50);
            addedCats.FailedEntities.Count.Should().Be(0);
            addedCats.IsSuccess.Should().BeTrue();
            addedCats.SuccessfulEntities.ToList().ForEach(entity => { cats.Should().Contain(entity); });

            addedDogs.Exception.Should().BeNull();
            addedDogs.SuccessfulEntities.Count.Should().Be(50);
            addedDogs.FailedEntities.Count.Should().Be(0);
            addedDogs.IsSuccess.Should().BeTrue();
            addedDogs.SuccessfulEntities.ToList().ForEach(entity => { dogs.Should().Contain(entity); });

            addeLions.Exception.Should().BeNull();
            addeLions.SuccessfulEntities.Count.Should().Be(50);
            addeLions.FailedEntities.Count.Should().Be(0);
            addeLions.IsSuccess.Should().BeTrue();
            addeLions.SuccessfulEntities.ToList().ForEach(entity => { lions.Should().Contain(entity); });

            addedCrocodiles.Exception.Should().BeNull();
            addedCrocodiles.SuccessfulEntities.Count.Should().Be(50);
            addedCrocodiles.FailedEntities.Count.Should().Be(0);
            addedCrocodiles.IsSuccess.Should().BeTrue();
            addedCrocodiles.SuccessfulEntities.ToList().ForEach(entity => { crocodiles.Should().Contain(entity); });
        }

        [Fact]
        public async Task WhenInvalidValidEntitiesAreAdded_ThenTheyFail()
        {
            var cats = new List<Cat>();
            var cosmosStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var id = Guid.NewGuid().ToString();
            await cosmosStore.AddAsync(new Cat {CatId = id, Name = "Nick"});

            for (var i = 0; i < 10; i++)
            {
                cats.Add(new Cat
                {
                    CatId = id
                });
            }

            var addedResults = await cosmosStore.AddRangeAsync(cats);

            addedResults.Exception.Should().BeNull();
            addedResults.SuccessfulEntities.Count.Should().Be(0);
            addedResults.FailedEntities.Count.Should().Be(10);
            addedResults.IsSuccess.Should().BeFalse();
            addedResults.FailedEntities.ToList().ForEach(entity =>
                {
                    entity.CosmosOperationStatus.Should().Be(CosmosOperationStatus.ResourceWithIdAlreadyExists);
                });
        }

        public void Dispose()
        {
            _cosmonautClient.DocumentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionName)).GetAwaiter().GetResult();
            _cosmonautClient.DocumentClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId)).GetAwaiter().GetResult();
        }
    }
}