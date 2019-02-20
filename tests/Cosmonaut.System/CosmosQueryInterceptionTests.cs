using Cosmonaut.System.Models;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cosmonaut.System
{
    public class CosmosQueryInterceptionTests : IDisposable
    {
        private readonly ICosmonautClient _cosmonautClient;
        private readonly Uri _emulatorUri = new Uri("https://localhost:8081");
        private readonly string _databaseId = $"DB{nameof(CosmosStoreTests)}";
        private readonly string _collectionName = $"COL{nameof(CosmosStoreTests)}";
        private readonly string _emulatorKey =
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly ConnectionPolicy _connectionPolicy = new ConnectionPolicy
        {
            ConnectionProtocol = Protocol.Tcp,
            ConnectionMode = ConnectionMode.Direct
        };

        public CosmosQueryInterceptionTests()
        {
            _cosmonautClient = new CosmonautClient(_emulatorUri, _emulatorKey, _connectionPolicy);
            Seed().GetAwaiter().GetResult();
        }

        [Fact]
        public void WhenInteceptorIsAppliedToQuery_ThenResultsShouldHaveOneItem()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.AddInterceptor<Cat>(x => x.Name == "Darren");
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings, _collectionName);

            cosmosStore.Query().ToList().Should().HaveCount(1);
        }

        [Fact]
        public void WhenInteceptorIsAppliedToAnotherType_ThenResultsShouldBeUnaffected()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.AddInterceptor<Dog>(x => x.Name == "Darren");
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings, _collectionName);

            cosmosStore.Query().ToList().Should().HaveCount(_cats.Count);
        }

        [Fact]
        public void WhenInteceptorIsAppliedAndQueryHaveFilterLogic_ThenThereShouldBeOneResult()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.AddInterceptor<Cat>(x => x.Name == "Nick");
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings, _collectionName);

            var results = cosmosStore.Query().Where(x => x.Name == "Nick").ToList();
            results.Should().HaveCount(1);
        }

        [Fact]
        public void WhenInteceptorIsAppliedAndHasMultipleQueryHaveFilterLogic_ThenThereShouldBeOneResult()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.AddInterceptor<Cat>(x => x.Name == "Nick");
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings, _collectionName);

            var results = cosmosStore.Query().Where(x => x.Name == "Nick").Where(x => x.Name == "Nick").ToList();
            results.Should().HaveCount(1);
            results.Single().Name.Should().BeEquivalentTo("Nick");
        }

        [Fact]
        public void WhenInteceptorIsAppliedAndHasSelect_ThenThereShouldBeOneResults()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.AddInterceptor<Cat>(x => x.Name == "Nick");
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings, _collectionName);

            var results = cosmosStore.Query().Select(x => x.Name).ToList();
            results.Should().HaveCount(1);
            results.Single().Equals("Nick");
        }

        [Fact]
        public void WhenInteceptorIsAppliedAndHasWhereAndSelect_ThenThereShouldBeOneResults()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.AddInterceptor<Cat>(x => x.Name == "Nick");
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings, _collectionName);

            var results = cosmosStore.Query().Where(x => x.Name == "Nick").Select(x => x.Name).ToList();
            results.Should().HaveCount(1);
            results.Single().Equals("Nick");
        }

        [Fact]
        public void WhenQueryInterceptorIsAppliedAndHasOrderBy_EnsureOneResult()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.AddInterceptor<Cat>(x => x.Name == "Nick");
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings, _collectionName);

            var results = cosmosStore.Query().OrderBy(x => x.Name).ToList();
            results.Should().HaveCount(1);
            results.Single().Name.Should().BeEquivalentTo("Nick");
        }

        [Fact]
        public void WhenInceptorUsesDateArithmetic_EnsureResultsAreFiltered()
        {
            var cosmosStoreSettings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey,
                settings =>
                {
                    settings.AddInterceptor<Cat>(x => x.DateOfBirth > new DateTime(1994, 1, 1));
                });

            var cosmosStore = new CosmosStore<Cat>(cosmosStoreSettings, _collectionName);
            var results = cosmosStore.Query().ToList();
            results.Should().HaveCount(2);
            results.Should().Contain(x => x.Name == "Darren");
            results.Should().Contain(x => x.Name == "Nick");
        }

        private async Task Seed()
        {
            var settings = new CosmosStoreSettings(_databaseId, _emulatorUri, _emulatorKey);
            var store = new CosmosStore<Cat>(settings, _collectionName);
            await store.AddRangeAsync(_cats);
        }

        public void Dispose()
        {
            _cosmonautClient.DeleteDatabaseAsync(_databaseId).GetAwaiter().GetResult();
        }

        private readonly List<Cat> _cats = new List<Cat>
        {
            new Cat
            {
                Name = "Darren",
                DateOfBirth = new DateTime(2000, 1, 1)
            },
            new Cat
            {
                Name = "Nick",
                DateOfBirth = new DateTime(1995, 1, 1)
            },
            new Cat
            {
                Name = "Tom",
                DateOfBirth = new DateTime(1990, 1, 1)
            }
        };
    }
}
