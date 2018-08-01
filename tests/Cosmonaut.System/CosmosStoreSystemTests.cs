using System;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
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

        private readonly ConnectionPolicy _connectionPolicy = new ConnectionPolicy
        {
            ConnectionProtocol = Protocol.Tcp,
            ConnectionMode = ConnectionMode.Direct
        };

        public CosmosStoreSystemTests()
        {
            _cosmonautClient = new CosmonautClient(_emulatorUri, _emulatorKey, _connectionPolicy);
        }

        [Fact]
        public async Task WhenCosmosStoreInitialised_ThenDatabaseAndCollectionIsCreated()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCosmosStore<Cat>(settings =>
            {
                settings.DatabaseName = _databaseId;
                settings.EndpointUrl = _emulatorUri;
                settings.AuthKey = _emulatorKey;
                settings.ConnectionPolicy = _connectionPolicy;
            }, _collectionName);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            serviceProvider.GetService<ICosmosStore<Cat>>();

            // Assert
            var database = await _cosmonautClient.GetDatabaseAsync(_databaseId);
            var collection = await _cosmonautClient.GetCollectionAsync(_databaseId, _collectionName);

            database.Should().NotBeNull();
            database.Id.Should().Be(_databaseId);
            collection.Should().NotBeNull();
            collection.Id.Should().Be(_collectionName);
        }



        public void Dispose()
        {
            _cosmonautClient.DocumentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionName)).GetAwaiter().GetResult();
            _cosmonautClient.DocumentClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId)).GetAwaiter().GetResult();
        }
    }
}