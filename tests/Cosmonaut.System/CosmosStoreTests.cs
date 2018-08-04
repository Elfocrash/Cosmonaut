using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Operations;
using Cosmonaut.Response;
using Cosmonaut.System.Models;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cosmonaut.System
{
    public class CosmosStoreTests : IDisposable
    {
        private readonly ICosmonautClient _cosmonautClient;
        private readonly Uri _emulatorUri = new Uri("https://localhost:8081");
        private readonly string _databaseId = $"DB{nameof(CosmosStoreTests)}";
        private readonly string _collectionName = $"COL{nameof(CosmosStoreTests)}";
        private readonly string _emulatorKey =
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly IServiceProvider _serviceProvider;

        private readonly ConnectionPolicy _connectionPolicy = new ConnectionPolicy
        {
            ConnectionProtocol = Protocol.Tcp,
            ConnectionMode = ConnectionMode.Direct
        };

        public CosmosStoreTests()
        {
            _cosmonautClient = new CosmonautClient(_emulatorUri, _emulatorKey, _connectionPolicy);
            var serviceCollection = new ServiceCollection();
            AddCosmosStores(serviceCollection);

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
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var birdStore = _serviceProvider.GetService<ICosmosStore<Bird>>();
            await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list));
            await ExecuteMultipleAddOperationsForType<Dog>(list => dogStore.AddRangeAsync(list));
            await ExecuteMultipleAddOperationsForType<Lion>(list => lionStore.AddRangeAsync(list));
            await ExecuteMultipleAddOperationsForType<Bird>(list => birdStore.AddRangeAsync(list));
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
        
        [Fact]
        public async Task WhenValidEntitiesAreRemoved_ThenRemovedResultsAreSuccessful()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var birdStore = _serviceProvider.GetService<ICosmosStore<Bird>>();
            var addedCats = await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list));
            var addedDogs = await ExecuteMultipleAddOperationsForType<Dog>(list => dogStore.AddRangeAsync(list));
            var addedLions = await ExecuteMultipleAddOperationsForType<Lion>(list => lionStore.AddRangeAsync(list));
            var addedBirds = await ExecuteMultipleAddOperationsForType<Bird>(list => birdStore.AddRangeAsync(list));

            await ExecuteMultipleAddOperationsForType(() => catStore.RemoveRangeAsync(addedCats.SuccessfulEntities.Select(x=>x.Entity)), HttpStatusCode.NoContent, addedCats.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => dogStore.RemoveRangeAsync(addedDogs.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.NoContent, addedDogs.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => lionStore.RemoveRangeAsync(addedLions.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.NoContent, addedLions.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => birdStore.RemoveRangeAsync(addedBirds.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.NoContent, addedBirds.SuccessfulEntities.Select(x => x.Entity).ToList());
        }

        [Fact]
        public async Task WhenAllEntitiesAreRemoved_ThenAllTheEntitiesAreRemoved()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var birdStore = _serviceProvider.GetService<ICosmosStore<Bird>>();
            await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list));
            await ExecuteMultipleAddOperationsForType<Dog>(list => dogStore.AddRangeAsync(list));
            await ExecuteMultipleAddOperationsForType<Lion>(list => lionStore.AddRangeAsync(list));
            await ExecuteMultipleAddOperationsForType<Bird>(list => birdStore.AddRangeAsync(list));

            await ExecuteMultipleAddOperationsForType(() => catStore.RemoveAsync(x => true), HttpStatusCode.NoContent);
            await ExecuteMultipleAddOperationsForType(() => dogStore.RemoveAsync(x => true), HttpStatusCode.NoContent);
            await ExecuteMultipleAddOperationsForType(() => lionStore.RemoveAsync(x => true), HttpStatusCode.NoContent);
            await ExecuteMultipleAddOperationsForType(() => birdStore.RemoveAsync(x => true), HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task WhenValidEntitiesAreUpdated_ThenUpdatedResultsAreSuccessful()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var birdStore = _serviceProvider.GetService<ICosmosStore<Bird>>();
            var addedCats = await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list));
            var addedDogs = await ExecuteMultipleAddOperationsForType<Dog>(list => dogStore.AddRangeAsync(list));
            var addedLions = await ExecuteMultipleAddOperationsForType<Lion>(list => lionStore.AddRangeAsync(list));
            var addedBirds = await ExecuteMultipleAddOperationsForType<Bird>(list => birdStore.AddRangeAsync(list));

            await ExecuteMultipleAddOperationsForType(() => catStore.UpdateRangeAsync(addedCats.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.OK, addedCats.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => dogStore.UpdateRangeAsync(addedDogs.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.OK, addedDogs.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => lionStore.UpdateRangeAsync(addedLions.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.OK, addedLions.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => birdStore.UpdateRangeAsync(addedBirds.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.OK, addedBirds.SuccessfulEntities.Select(x => x.Entity).ToList());
        }

        [Fact]
        public async Task WhenValidEntitiesAreUpserted_ThenUpsertedResultsAreSuccessful()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var birdStore = _serviceProvider.GetService<ICosmosStore<Bird>>();
            var addedCats = await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list));
            var addedDogs = await ExecuteMultipleAddOperationsForType<Dog>(list => dogStore.AddRangeAsync(list));
            var addedLions = await ExecuteMultipleAddOperationsForType<Lion>(list => lionStore.AddRangeAsync(list));
            var addedBird = await ExecuteMultipleAddOperationsForType<Bird>(list => birdStore.AddRangeAsync(list));

            await ExecuteMultipleAddOperationsForType(() => catStore.UpsertRangeAsync(addedCats.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.OK, addedCats.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => dogStore.UpsertRangeAsync(addedDogs.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.OK, addedDogs.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => lionStore.UpsertRangeAsync(addedLions.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.OK, addedLions.SuccessfulEntities.Select(x => x.Entity).ToList());
            await ExecuteMultipleAddOperationsForType(() => birdStore.UpsertRangeAsync(addedBird.SuccessfulEntities.Select(x => x.Entity)), HttpStatusCode.OK, addedBird.SuccessfulEntities.Select(x => x.Entity).ToList());
        }

        [Fact]
        public async Task WhenValidEntitiesAreAdded_ThenTheyCanBeQueriedFor()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var birdStore = _serviceProvider.GetService<ICosmosStore<Bird>>();
            var addedCats = await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list));
            var addedDogs = await ExecuteMultipleAddOperationsForType<Dog>(list => dogStore.AddRangeAsync(list));
            var addedLions = await ExecuteMultipleAddOperationsForType<Lion>(list => lionStore.AddRangeAsync(list));
            var addedBirds = await ExecuteMultipleAddOperationsForType<Bird>(list => birdStore.AddRangeAsync(list));

            var cats = await catStore.Query().ToListAsync();
            var dogs = await dogStore.QueryMultipleAsync<Dog>("select * from c");
            var lions = await lionStore.QueryMultipleAsync("select * from c");
            var birds = await birdStore.Query().ToListAsync();

            cats.Should().BeEquivalentTo(addedCats.SuccessfulEntities.Select(x=>x.Entity));
            dogs.Should().BeEquivalentTo(addedDogs.SuccessfulEntities.Select(x => x.Entity));
            lions.Should().BeEquivalentTo(addedLions.SuccessfulEntities.Select(x => x.Entity), config =>
            {
                config.Excluding(x => x.CosmosEntityName);
                return config;
            });
            birds.Should().BeEquivalentTo(addedBirds.SuccessfulEntities.Select(x => x.Entity), config =>
            {
                config.Excluding(x => x.CosmosEntityName);
                return config;
            });
        }

        [Fact]
        public async Task WhenCollectionIsUpScaled_AndAutomaticScalingIsTurnedOff_ThenOfferDoesNotChange()
        {
            var catStore = new CosmosStore<Cat>(new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey), _collectionName);
            var cosmosScaler = new CosmosScaler<Cat>(catStore);

            await cosmosScaler.UpscaleCollectionRequestUnitsForRequest(_databaseId, _collectionName, 100, 5);

            var offer = await catStore.CosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            offer.Content.OfferThroughput.Should().Be(400);
        }

        [Fact]
        public async Task WhenCollectionIsUpScaled_AndAutomaticScalingIsTurnedOn_ThenOfferIsUpscaled()
        {
            var catStore = new CosmosStore<Cat>(new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.DefaultCollectionThroughput = 500;
                    settings.ScaleCollectionRUsAutomatically = true;
                }), _collectionName);
            var cosmosScaler = new CosmosScaler<Cat>(catStore);

            await cosmosScaler.UpscaleCollectionRequestUnitsForRequest(_databaseId, _collectionName, 100, 5);

            var offer = await catStore.CosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            offer.Content.OfferThroughput.Should().Be(500);
        }

        [Fact]
        public async Task WhenCollectionIsDownScaled_AndAutomaticScalingIsTurnedOff_ThenOfferDoesNotChange()
        {
            var catStore = new CosmosStore<Cat>(new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.DefaultCollectionThroughput = 500;
                }), _collectionName);
            var cosmosScaler = new CosmosScaler<Cat>(catStore);

            var preScaleOffer = await catStore.CosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            await cosmosScaler.DownscaleCollectionRequestUnitsToDefault(_databaseId, _collectionName);

            var postScaleOffer = await catStore.CosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            preScaleOffer.Content.OfferThroughput.Should().Be(500);
            postScaleOffer.Content.OfferThroughput.Should().Be(500);
        }

        [Fact]
        public async Task WhenCollectionIsDownScaled_AndAutomaticScalingIsTurnedOn_ThenOfferIsDownscaled()
        {
            var catStore = new CosmosStore<Cat>(new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.DefaultCollectionThroughput = 500;
                    settings.ScaleCollectionRUsAutomatically = true;
                }), _collectionName);
            var cosmosScaler = new CosmosScaler<Cat>(catStore);

            await cosmosScaler.UpscaleCollectionRequestUnitsForRequest(_databaseId, _collectionName, 100, 6);
            var preScaleOffer = await catStore.CosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            await cosmosScaler.DownscaleCollectionRequestUnitsToDefault(_databaseId, _collectionName);

            var postScaleOffer = await catStore.CosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            preScaleOffer.Content.OfferThroughput.Should().Be(600);
            postScaleOffer.Content.OfferThroughput.Should().Be(500);
        }

        [Fact]
        public async Task WhenRUIntenseOperationHappens_AndAutomaticScalingIsTurnedOn_ThenOfferUpscaledAndDownscaled()
        {
            var catStore = new CosmosStore<Cat>(new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.DefaultCollectionThroughput = 400;
                    settings.ScaleCollectionRUsAutomatically = true;
                }), _collectionName);
            await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list), 400);


            var postScaleOffer = await catStore.CosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);
            postScaleOffer.Content.OfferThroughput.Should().Be(400);
        }

        private async Task<CosmosMultipleResponse<T>> ExecuteMultipleAddOperationsForType<T>(
            Func<IEnumerable<T>, Task<CosmosMultipleResponse<T>>> operationFunc, int itemCount = 50) 
            where T : Animal, new()
        {
            var items = new List<T>();
            
            for (var i = 0; i < itemCount; i++){items.Add(new T { Name = Guid.NewGuid().ToString() });}

            var addedCats = await operationFunc(items);

            addedCats.Exception.Should().BeNull();
            addedCats.SuccessfulEntities.Count.Should().Be(itemCount);
            addedCats.FailedEntities.Count.Should().Be(0);
            addedCats.IsSuccess.Should().BeTrue();
            addedCats.SuccessfulEntities.ToList().ForEach(entity => { items.Should().Contain(entity); });

            return addedCats;
        }

        private async Task ExecuteMultipleAddOperationsForType<T>(Func<Task<CosmosMultipleResponse<T>>> operationFunc, HttpStatusCode expectedCode, List<T> entitiesToAssert = null)
            where T : Animal, new()
        {
            var addedCats = await operationFunc();

            addedCats.Exception.Should().BeNull();
            addedCats.SuccessfulEntities.Count.Should().Be(50);
            addedCats.FailedEntities.Count.Should().Be(0);
            addedCats.IsSuccess.Should().BeTrue();
            addedCats.SuccessfulEntities.ForEach(cat =>
            {
                cat.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
                cat.ResourceResponse.StatusCode.Should().Be(expectedCode);
            });

            if (entitiesToAssert != null)
            {
                addedCats.SuccessfulEntities.ToList().ForEach(entity =>
                {
                    entitiesToAssert.Should().Contain(entity);
                });
            }
        }

        public void Dispose()
        {
            _cosmonautClient.DeleteCollectionAsync(_databaseId, _collectionName).GetAwaiter().GetResult();
            _cosmonautClient.DeleteDatabaseAsync(_databaseId).GetAwaiter().GetResult();
        }
        
        private void AddCosmosStores(ServiceCollection serviceCollection)
        {
            serviceCollection.AddCosmosStore<Cat>(_databaseId, _emulatorUri, _emulatorKey,
                settings => { settings.ConnectionPolicy = _connectionPolicy; }, _collectionName);
            serviceCollection.AddCosmosStore<Dog>(_databaseId, _emulatorUri, _emulatorKey,
                    settings => { settings.ConnectionPolicy = _connectionPolicy; })
                .AddCosmosStore<Lion>(_databaseId, _emulatorUri, _emulatorKey,
                    settings => { settings.ConnectionPolicy = _connectionPolicy; })
                .AddCosmosStore<Bird>(_databaseId, _emulatorUri, _emulatorKey,
                    settings => { settings.ConnectionPolicy = _connectionPolicy; });
        }
    }
}