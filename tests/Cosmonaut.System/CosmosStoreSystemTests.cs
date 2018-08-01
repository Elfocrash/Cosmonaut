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
            serviceCollection.AddCosmosStore<Cat>(settings =>
            {
                settings.DatabaseName = _databaseId;
                settings.EndpointUrl = _emulatorUri;
                settings.AuthKey = _emulatorKey;
                settings.ConnectionPolicy = _connectionPolicy;
            }, _collectionName);

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
            var cats = new List<Cat>();
            var cosmosStore = _serviceProvider.GetService<ICosmosStore<Cat>>();
            for (var i = 0; i < 50; i++)
            {
                cats.Add(new Cat
                {
                    Name = Guid.NewGuid().ToString()
                });
            }

            var addedResults = await cosmosStore.AddRangeAsync(cats);

            addedResults.Exception.Should().BeNull();
            addedResults.SuccessfulEntities.Count.Should().Be(50);
            addedResults.FailedEntities.Count.Should().Be(0);
            addedResults.IsSuccess.Should().BeTrue();
            addedResults.SuccessfulEntities.ToList().ForEach(entity =>
            {
                cats.Should().Contain(entity);
            });
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