using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Cosmonaut.Operations;
using Cosmonaut.Response;
using Cosmonaut.System.Models;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
        public async Task WhenEntitiesAreAddedAndIdExists_ThenTheyFail()
        {
            var cats = new List<Cat>();
            var cosmosStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var id = Guid.NewGuid().ToString();
            await cosmosStore.AddAsync(new Cat { CatId = id, Name = "Nick" });

            for (var i = 0; i < 10; i++)
            {
                cats.Add(new Cat
                {
                    CatId = id
                });
            }

            var addedResults = await cosmosStore.AddRangeAsync(cats);

            addedResults.IsSuccess.Should().BeFalse();
            addedResults.FailedEntities.Count.Should().Be(10);
            addedResults.SuccessfulEntities.Count.Should().Be(0);
            addedResults.FailedEntities.Select(x=>x.CosmosOperationStatus).Should().AllBeEquivalentTo(CosmosOperationStatus.Conflict);
        }

        [Fact]
        public async Task WhenEntitiesAreAddedAndTheyChangedWithAccessCondition_ThenTheyFail()
        {
            var cosmosStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var response = await ExecuteMultipleAddOperationsForType<Cat>(list => cosmosStore.AddRangeAsync(list), 10);
            
            var addedCats = response.SuccessfulEntities
                .Select(x => JsonConvert.DeserializeObject<Cat>(x.ResourceResponse.Resource.ToString())).ToList();
                addedCats.ForEach(x => x.Name = "different Name");
            await cosmosStore.UpdateRangeAsync(addedCats);

            var updatedResults = await cosmosStore.UpdateRangeAsync(addedCats, cat => new RequestOptions{AccessCondition = new AccessCondition
            {
                Type = AccessConditionType.IfMatch,
                Condition = cat.Etag
            }});

            response.IsSuccess.Should().BeTrue();
            response.FailedEntities.Count.Should().Be(0);
            response.SuccessfulEntities.Count.Should().Be(10);
            updatedResults.IsSuccess.Should().BeFalse();
            updatedResults.FailedEntities.Count.Should().Be(10);
            updatedResults.SuccessfulEntities.Count.Should().Be(0);
            updatedResults.FailedEntities.Select(x => x.CosmosOperationStatus).Should().AllBeEquivalentTo(CosmosOperationStatus.PreconditionFailed);
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

            cats.Should().BeEquivalentTo(addedCats.SuccessfulEntities.Select(x=>x.Entity), ExcludeEtagCheck());
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
        public async Task WhenValidEntitiesAreAdded_ThenTheyCanBeFoundAsync()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var birdStore = _serviceProvider.GetService<ICosmosStore<Bird>>();
            var alpacaStore = _serviceProvider.GetService<ICosmosStore<Alpaca>>();
            var addedCats = await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list), 1);
            var addedDogs = await ExecuteMultipleAddOperationsForType<Dog>(list => dogStore.AddRangeAsync(list), 1);
            var addedLions = await ExecuteMultipleAddOperationsForType<Lion>(list => lionStore.AddRangeAsync(list), 1);
            var addedBirds = await ExecuteMultipleAddOperationsForType<Bird>(list => birdStore.AddRangeAsync(list), 1);
            var addedAlpacas = await ExecuteMultipleAddOperationsForType<Alpaca>(list => alpacaStore.AddRangeAsync(list), 1);

            var catFound = await catStore.FindAsync(addedCats.SuccessfulEntities.Single().Entity.CatId, addedCats.SuccessfulEntities.Single().Entity.CatId);
            var dogFound = await dogStore.FindAsync(addedDogs.SuccessfulEntities.Single().Entity.Id);
            var lionFound = await lionStore.FindAsync(addedLions.SuccessfulEntities.Single().Entity.Id, addedLions.SuccessfulEntities.Single().Entity.Id);
            var birdFound = await birdStore.FindAsync(addedBirds.SuccessfulEntities.Single().Entity.Id, addedBirds.SuccessfulEntities.Single().Entity.Id);
            var alpacaFound = await alpacaStore.FindAsync(addedAlpacas.SuccessfulEntities.Single().Entity.Id);

            catFound.Should().BeEquivalentTo(JsonConvert.DeserializeObject<Cat>(addedCats.SuccessfulEntities.Single().ResourceResponse.Resource.ToString()));
            dogFound.Should().BeEquivalentTo(JsonConvert.DeserializeObject<Dog>(addedDogs.SuccessfulEntities.Single().ResourceResponse.Resource.ToString()));
            lionFound.Should().BeEquivalentTo(JsonConvert.DeserializeObject<Lion>(addedLions.SuccessfulEntities.Single().ResourceResponse.Resource.ToString()));
            birdFound.Should().BeEquivalentTo(JsonConvert.DeserializeObject<Bird>(addedBirds.SuccessfulEntities.Single().ResourceResponse.Resource.ToString()));
            alpacaFound.Should().BeEquivalentTo(JsonConvert.DeserializeObject<Alpaca>(addedAlpacas.SuccessfulEntities.Single().ResourceResponse.Resource.ToString()));
        }

        [Fact]
        public async Task WhenValidEntitiesAreNotAdded_ThenTheyCanNotBeFoundAsync()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var dogStore = _serviceProvider.GetService<ICosmosStore<Dog>>();
            var lionStore = _serviceProvider.GetService<ICosmosStore<Lion>>();
            var birdStore = _serviceProvider.GetService<ICosmosStore<Bird>>();
            var alpacaStore = _serviceProvider.GetService<ICosmosStore<Alpaca>>();

            var catFound = await catStore.FindAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var dogFound = await dogStore.FindAsync(Guid.NewGuid().ToString());
            var lionFound = await lionStore.FindAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var birdFound = await birdStore.FindAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var alpacaFound = await alpacaStore.FindAsync(Guid.NewGuid().ToString());

            catFound.Should().BeNull();
            dogFound.Should().BeNull();
            lionFound.Should().BeNull();
            birdFound.Should().BeNull();
            alpacaFound.Should().BeNull();
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

        [Fact]
        public async Task WhenPaginatedQueryExecutesWithSkipTake_ThenPaginatedResultsAreReturnedCorrectly()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var addedCats = (await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list), 15))
                .SuccessfulEntities.Select(x=>x.Entity).OrderBy(x=>x.Name).ToList();

            var firstPage = await catStore.Query().WithPagination(1, 5).OrderBy(x=>x.Name).ToListAsync();
            var secondPage = await catStore.Query().WithPagination(2, 5).OrderBy(x => x.Name).ToListAsync();
            var thirdPage = await catStore.Query().WithPagination(3, 5).OrderBy(x => x.Name).ToListAsync();
            var fourthPage = await catStore.Query().WithPagination(4, 5).OrderBy(x => x.Name).ToListAsync();

            firstPage.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Take(5), ExcludeEtagCheck());
            secondPage.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Skip(5).Take(5), ExcludeEtagCheck());
            thirdPage.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Skip(10).Take(5), ExcludeEtagCheck());
            fourthPage.Should().BeEmpty();
        }

        [Fact]
        public async Task WhenPaginatedQueryExecutesWithNextPageAsync_ThenPaginatedResultsAreReturnedCorrectly()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var addedCats = (await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list), 15))
                .SuccessfulEntities.Select(x => x.Entity).OrderBy(x => x.Name).ToList();

            var firstPage = await catStore.Query().WithPagination(1, 5).OrderBy(x => x.Name).ToPagedListAsync();
            var secondPage = await firstPage.GetNextPageAsync();
            var thirdPage = await secondPage.GetNextPageAsync();
            var fourthPage = await thirdPage.GetNextPageAsync();

            firstPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Take(5), ExcludeEtagCheck());
            secondPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Skip(5).Take(5), ExcludeEtagCheck());
            thirdPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Skip(10).Take(5), ExcludeEtagCheck());
            fourthPage.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task WhenPaginatedQueryAndFeedOptionsExecutesWithNextPageAsync_ThenPaginatedResultsAreReturnedCorrectly()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var addedCats = (await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list), 15))
                .SuccessfulEntities.Select(x => x.Entity).OrderBy(x => x.Name).ToList();

            var firstPage = await catStore.Query(new FeedOptions{ RequestContinuation = "SomethingBad", MaxItemCount = 666}).WithPagination(1, 5).OrderBy(x => x.Name).ToPagedListAsync();
            var secondPage = await firstPage.GetNextPageAsync();
            var thirdPage = await secondPage.GetNextPageAsync();
            var fourthPage = await thirdPage.GetNextPageAsync();

            firstPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Take(5), ExcludeEtagCheck());
            secondPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Skip(5).Take(5), ExcludeEtagCheck());
            thirdPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Skip(10).Take(5), ExcludeEtagCheck());
            fourthPage.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task WhenPaginatedQueryExecutesWithContinuationToken_ThenPaginatedResultsAreReturnedCorrectly()
        {
            var catStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            var addedCats = (await ExecuteMultipleAddOperationsForType<Cat>(list => catStore.AddRangeAsync(list), 30))
                .SuccessfulEntities.Select(x => x.Entity).OrderBy(x => x.Name).ToList();

            var firstPage = await catStore.Query().WithPagination(1, 10).OrderBy(x => x.Name).ToPagedListAsync();
            var secondPage = await catStore.Query().WithPagination(firstPage.NextPageToken, 10).OrderBy(x => x.Name).ToPagedListAsync();
            var thirdPage = await catStore.Query().WithPagination(secondPage.NextPageToken, 10).OrderBy(x => x.Name).ToPagedListAsync();
            var fourthPage = await catStore.Query().WithPagination(4, 10).OrderBy(x => x.Name).ToPagedListAsync();
            var emptyTokenPage = await catStore.Query().WithPagination(null, 10).OrderBy(x => x.Name).ToPagedListAsync();

            firstPage.HasNextPage.Should().BeTrue();
            firstPage.NextPageToken.Should().NotBeNullOrEmpty();
            firstPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Take(10), ExcludeEtagCheck());
            secondPage.HasNextPage.Should().BeTrue();
            secondPage.NextPageToken.Should().NotBeNullOrEmpty();
            secondPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Skip(10).Take(10), ExcludeEtagCheck());
            thirdPage.HasNextPage.Should().BeFalse();
            thirdPage.NextPageToken.Should().BeNullOrEmpty();
            thirdPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Skip(20).Take(10), ExcludeEtagCheck());
            fourthPage.Results.Should().BeEmpty();
            fourthPage.NextPageToken.Should().BeNullOrEmpty();
            fourthPage.HasNextPage.Should().BeFalse();
            emptyTokenPage.HasNextPage.Should().BeTrue();
            emptyTokenPage.NextPageToken.Should().NotBeNullOrEmpty();
            emptyTokenPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(addedCats.Take(10), ExcludeEtagCheck());
        }

        private async Task<CosmosMultipleResponse<T>> ExecuteMultipleAddOperationsForType<T>(
            Func<IEnumerable<T>, Task<CosmosMultipleResponse<T>>> operationFunc, int itemCount = 50) 
            where T : Animal, new()
        {
            var items = new List<T>();
            
            for (var i = 0; i < itemCount; i++){items.Add(new T { Name = Guid.NewGuid().ToString() });}

            var addedCats = await operationFunc(items);

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
            serviceCollection
                .AddCosmosStore<Cat>(_databaseId, _emulatorUri, _emulatorKey,
                    settings =>
                    {
                        settings.ConnectionPolicy = _connectionPolicy;
                        settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1), new RangeIndex(DataType.String, -1));
                    }, _collectionName)
                .AddCosmosStore<Dog>(_databaseId, _emulatorUri, _emulatorKey,
                    settings =>
                    {
                        settings.ConnectionPolicy = _connectionPolicy;
                        settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1), new RangeIndex(DataType.String, -1));
                    })
                .AddCosmosStore<Lion>(_databaseId, _emulatorUri, _emulatorKey,
                    settings =>
                    {
                        settings.ConnectionPolicy = _connectionPolicy;
                        settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1), new RangeIndex(DataType.String, -1));
                    })
                .AddCosmosStore<Bird>(_databaseId, _emulatorUri, _emulatorKey,
                    settings =>
                    {
                        settings.ConnectionPolicy = _connectionPolicy;
                        settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1), new RangeIndex(DataType.String, -1));
                    })
                .AddCosmosStore<Alpaca>(_databaseId, _emulatorUri, _emulatorKey,
                    settings =>
                    {
                        settings.ConnectionPolicy = _connectionPolicy;
                        settings.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1), new RangeIndex(DataType.String, -1));
                    });
        }

        private static Func<EquivalencyAssertionOptions<Cat>, EquivalencyAssertionOptions<Cat>> ExcludeEtagCheck()
        {
            return config =>
            {
                config.Excluding(cat => cat.Etag);
                return config;
            };
        }
    }
}