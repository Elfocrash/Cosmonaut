using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Cosmonaut.Configuration;
using Cosmonaut.Exceptions;
using Cosmonaut.Storage;
using Cosmonaut.System.Models;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Xunit;

namespace Cosmonaut.System
{
    public class CosmosProvisioningTests : IDisposable
    {
        private readonly ICosmonautClient _cosmonautClient;
        private readonly string _emulatorUri = Environment.GetEnvironmentVariable("CosmosDBEndpoint") ?? "https://localhost:8081";
        private readonly string _emulatorKey =
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private readonly string _databaseId = $"DB{nameof(CosmosProvisioningTests)}";
        private readonly string _collectionName = $"COL{nameof(CosmosProvisioningTests)}";

        private readonly ConnectionPolicy _connectionPolicy = new ConnectionPolicy
        {
            ConnectionProtocol = Protocol.Tcp,
            ConnectionMode = ConnectionMode.Direct
        };

        public CosmosProvisioningTests()
        {
            _cosmonautClient = new CosmonautClient(_emulatorUri, _emulatorKey, _connectionPolicy);
        }

        [Fact]
        public async Task DatabaseCreator_CreatesDatabaseWithoutThroughput_WhenThroughputNull()
        {
            var databaseCreator = new CosmosDatabaseCreator(_cosmonautClient);

            var created = await databaseCreator.EnsureCreatedAsync(_databaseId);
            var offer = await _cosmonautClient.GetOfferV2ForDatabaseAsync(_databaseId);

            created.Should().BeTrue();
            offer.Should().BeNull();
        }

        [Fact]
        public async Task DatabaseCreator_CreatesDatabaseWithThroughput_WhenThroughputIsPresent()
        {
            var databaseCreator = new CosmosDatabaseCreator(_cosmonautClient);

            var created = await databaseCreator.EnsureCreatedAsync(_databaseId, 10000);
            var offer = await _cosmonautClient.GetOfferV2ForDatabaseAsync(_databaseId);

            created.Should().BeTrue();
            offer.Content.OfferThroughput.Should().Be(10000);
        }

        [Fact]
        public async Task CollectionCreator_CreatesCollectionWithThroughput_WhenThroughputIsPresent()
        {
            var databaseCreator = new CosmosDatabaseCreator(_cosmonautClient);
            var collectionCreator = new CosmosCollectionCreator(_cosmonautClient);
            await databaseCreator.EnsureCreatedAsync(_databaseId);
            await collectionCreator.EnsureCreatedAsync<object>(_databaseId, _collectionName, 10000);

            var offer = await _cosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            offer.Content.OfferThroughput.Should().Be(10000);
        }

        [Fact]
        public async Task CollectionCreator_CreatesCollectionWithoutThroughput_WhenThroughputIsPresentOnDatabase()
        {
            var databaseCreator = new CosmosDatabaseCreator(_cosmonautClient);
            var collectionCreator = new CosmosCollectionCreator(_cosmonautClient);
            await databaseCreator.EnsureCreatedAsync(_databaseId, 20000);
            await collectionCreator.EnsureCreatedAsync<Cat>(_databaseId, _collectionName, 10000);

            var databaseOffer = await _cosmonautClient.GetOfferV2ForDatabaseAsync(_databaseId);
            var collectionOffer = await _cosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            databaseOffer.Content.OfferThroughput.Should().Be(20000);
            collectionOffer.Should().BeNull();
        }

        [Fact]
        public async Task CollectionCreator_CreatesCollectionWithDedicatedThroughput_WhenThroughputIsPresentOnDatabase()
        {
            var databaseCreator = new CosmosDatabaseCreator(_cosmonautClient);
            var collectionCreator = new CosmosCollectionCreator(_cosmonautClient);
            await databaseCreator.EnsureCreatedAsync(_databaseId, 20000);
            await collectionCreator.EnsureCreatedAsync<Cat>(_databaseId, _collectionName, 10000, onDatabaseBehaviour: ThroughputBehaviour.DedicateCollectionThroughput);

            var databaseOffer = await _cosmonautClient.GetOfferV2ForDatabaseAsync(_databaseId);
            var collectionOffer = await _cosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            databaseOffer.Content.OfferThroughput.Should().Be(20000);
            collectionOffer.Content.OfferThroughput.Should().Be(10000);
        }

        [Fact]
        public async Task CosmosStoreSettings_CreatesCollectionWithThroughput_WhenThroughputIsPresent()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey, settings =>
                {
                    settings.ConnectionPolicy = _connectionPolicy;
                    settings.DefaultCollectionThroughput = 10000;
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings);

            var databaseOffer = await _cosmonautClient.GetOfferV2ForDatabaseAsync(cosmosStore.DatabaseName);
            var collectionOffer = await _cosmonautClient.GetOfferV2ForCollectionAsync(cosmosStore.DatabaseName, cosmosStore.CollectionName);

            databaseOffer.Should().BeNull();
            collectionOffer.Content.OfferThroughput.Should().Be(10000);
        }

        [Fact]
        public async Task CosmosStoreSettings_CreatesCollectionWithoutThroughput_WhenThroughputIsPresentOnDatabase()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey, settings =>
            {
                settings.ConnectionPolicy = _connectionPolicy;
                settings.DefaultCollectionThroughput = 10000;
                settings.DefaultDatabaseThroughput = 20000;
            });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings);

            var databaseOffer = await _cosmonautClient.GetOfferV2ForDatabaseAsync(cosmosStore.DatabaseName);
            var collectionOffer = await _cosmonautClient.GetOfferV2ForCollectionAsync(cosmosStore.DatabaseName, cosmosStore.CollectionName);

            databaseOffer.Content.OfferThroughput.Should().Be(20000);
            collectionOffer.Should().BeNull();
        }

        [Fact]
        public async Task CosmosStoreSettings_CreatesCollectionWithThroughput_WhenThroughputIsPresentOnDatabaseAndEnforcedOnCollection()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey, settings =>
            {
                settings.ConnectionPolicy = _connectionPolicy;
                settings.DefaultCollectionThroughput = 10000;
                settings.DefaultDatabaseThroughput = 20000;
                settings.OnDatabaseThroughput = ThroughputBehaviour.DedicateCollectionThroughput;
            });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings);

            var databaseOffer = await _cosmonautClient.GetOfferV2ForDatabaseAsync(cosmosStore.DatabaseName);
            var collectionOffer = await _cosmonautClient.GetOfferV2ForCollectionAsync(cosmosStore.DatabaseName, cosmosStore.CollectionName);

            databaseOffer.Content.OfferThroughput.Should().Be(20000);
            collectionOffer.Content.OfferThroughput.Should().Be(10000);
        }

        [Fact]
        public async Task CosmosStoreSettings_CreatesCollectionWithUniqueKeyPolicy()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey, settings =>
            {
                settings.ProvisionInfrastructureIfMissing = true;
                settings.ConnectionPolicy = _connectionPolicy;
                settings.UniqueKeyPolicy = new UniqueKeyPolicy
                                           {
                                               UniqueKeys =
                                               {
                                                   new UniqueKey
                                                   {
                                                       Paths =
                                                       {
                                                           "/name",
                                                           "/bladiebla"
                                                       }
                                                   }
                                               }
                                           };
            });

            var store = new CosmosStore<Lion>(cosmosStoreSettings);
            var collection = await store.CosmonautClient.GetCollectionAsync(_databaseId, store.CollectionName);

            collection.UniqueKeyPolicy.UniqueKeys.Should().HaveCount(1);
        }

         [Fact]
        public async Task CosmosStoreSettings_CreatesCollectionWithSharedEntity_WhenNoUniqueKeyPolicyIsDefined()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey, settings =>
                                                                                                       {
                                                                                                           settings.ProvisionInfrastructureIfMissing = true;
                                                                                                           settings.ConnectionPolicy = _connectionPolicy;
                                                                                                       });

            var store = new CosmosStore<Lion>(cosmosStoreSettings);
            var collection = await store.CosmonautClient.GetCollectionAsync(_databaseId, store.CollectionName);

            collection.UniqueKeyPolicy.UniqueKeys.Should().HaveCount(0);
        }

        public void Dispose()
        {
            _cosmonautClient.DeleteDatabaseAsync(_databaseId).GetAwaiter().GetResult();
        }
    }
}