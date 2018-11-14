using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Cosmonaut.System.Models;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Xunit;

namespace Cosmonaut.System
{
    public class CosmonautClientTests : IDisposable
    {
        private readonly ICosmonautClient _cosmonautClient;
        private readonly Uri _emulatorUri = new Uri("https://localhost:8081");
        private readonly string _databaseId = $"DB{nameof(CosmonautClientTests)}";
        private readonly string _collectionName = $"COL{nameof(CosmonautClientTests)}";
        private readonly string _emulatorKey =
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public CosmonautClientTests()
        {
            _cosmonautClient = new CosmonautClient(_emulatorUri, _emulatorKey, new ConnectionPolicy
            {
                ConnectionProtocol = Protocol.Tcp,
                ConnectionMode = ConnectionMode.Direct
            });

            _cosmonautClient.CreateDatabaseAsync(new Database { Id = _databaseId }).GetAwaiter().GetResult();
            _cosmonautClient.CreateCollectionAsync(_databaseId, new DocumentCollection
            {
                Id = _collectionName,
                IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.Number, -1), new RangeIndex(DataType.String, -1))
            }).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task QueryDatabasesAsync_WhenQueryingASingleDatabase_ThenTheDatabaseGetReturned()
        {
            // Act
            var databases = (await _cosmonautClient.QueryDatabasesAsync(x => x.Id == _databaseId)).ToList();

            // Assert
            databases.Count.Should().Be(1);
            databases.Single().Id.Should().Be(_databaseId);
        }

        [Fact]
        public async Task QueryDatabasesAsync_WhenQueryingAllDatabases_ThenAllDatabasesGetReturned()
        {
            // Arrange
            await _cosmonautClient.CreateDatabaseAsync(new Database { Id = "Nick" });
            await _cosmonautClient.CreateDatabaseAsync(new Database { Id = "TheGreek" });

            // Act
            var databases = (await _cosmonautClient.QueryDatabasesAsync()).ToList();

            // Assert
            databases.Select(x => x.Id).Should().Contain("Nick");
            databases.Select(x => x.Id).Should().Contain("TheGreek");

            // Cleanup
            await _cosmonautClient.DeleteDatabaseAsync("Nick");
            await _cosmonautClient.DeleteDatabaseAsync("TheGreek");
        }
        
        [Fact]
        public async Task QueryOffersAsync_WhenQueryingOffers_ThenAllOffersGetReturned()
        {
            // Arrange
            var collections = await _cosmonautClient.QueryCollectionsAsync(_databaseId);

            // Act
            var offers = await _cosmonautClient.QueryOffersAsync();

            // Assert
            collections.ToList().ForEach(collection =>
            {
                offers.Select(x => x.ResourceLink).Contains(collection.SelfLink).Should().BeTrue();
            });
        }

        [Fact]
        public async Task UpdateOfferAsync_WhenOfferIsUpdated_ThenRUsCorrespondToTheUpdate()
        {
            // Arrange
            var offer = await _cosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            // Act
            var newOffer = new OfferV2(offer, 600);
            var updated = await _cosmonautClient.UpdateOfferAsync(newOffer);
            var queried = await _cosmonautClient.GetOfferV2ForCollectionAsync(_databaseId, _collectionName);

            // Assert
            offer.Content.OfferThroughput.Should().Be(400);
            updated.StatusCode.Should().Be(HttpStatusCode.OK);
            queried.Content.OfferThroughput.Should().Be(600);
            updated.Resource.Should().BeEquivalentTo(queried);
        }

        [Fact]
        public async Task QueryOffersV2Async_WhenQueryingOffersV2_ThenAllOffersV2GetReturned()
        {
            // Arrange
            var collections = await _cosmonautClient.QueryCollectionsAsync(_databaseId);

            // Act
            var offersV2 = await _cosmonautClient.QueryOffersV2Async();

            // Assert
            collections.ToList().ForEach(collection =>
            {
                offersV2.Select(x => x.ResourceLink).Contains(collection.SelfLink).Should().BeTrue();
            });
        }

        [Fact]
        public async Task QueryDocumentsAsync_WhenQueryingWithLINQ_ThenObjectsGetReturned()
        {
            // Arrange
            var cats = new List<Cat>();
            for (var i = 0; i < 5; i++)
            {
                var cat = new Cat {Name = $"Cat {i}"};
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(created.Entity);
            }

            // Act
            var results = await _cosmonautClient.QueryDocumentsAsync<Cat>(_databaseId, _collectionName, x => x.Name.StartsWith("Cat "),
                new FeedOptions{EnableScanInQuery = true});

            // Assert
            results.Should().BeEquivalentTo(cats, ExcludeEtagCheck());
        }

        [Fact]
        public async Task QueryDocumentsAsync_WhenQueryingWithSQL_ThenObjectsGetReturned()
        {
            // Arrange
            var cats = new List<Cat>();
            for (var i = 0; i < 5; i++)
            {
                var cat = new Cat { Name = $"Cat {i}" };
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(created.Entity);
            }

            // Act
            var results = await _cosmonautClient.QueryDocumentsAsync<Cat>(_databaseId, _collectionName,
                "select * from c where STARTSWITH(c.Name, @catName)", new { catName = "Cat" },
                new FeedOptions { EnableScanInQuery = true });

            // Assert
            results.Should().BeEquivalentTo(cats, ExcludeEtagCheck());
        }

        [Fact]
        public async Task QueryDocumentsAsync_WhenQueryingAllObjects_ThenAllObjectsGetReturned()
        {
            // Arrange
            var cats = new List<Cat>();
            for (var i = 0; i < 5; i++)
            {
                var cat = new Cat { Name = $"Cat {i}" };
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(created.Entity);
            }

            // Act
            var results = (await _cosmonautClient.QueryDocumentsAsync<Cat>(_databaseId, _collectionName)).ToList();

            // Assert
            results.Should().BeEquivalentTo(cats, ExcludeEtagCheck());
        }

        [Fact]
        public async Task QueryDocumentsAsync_WhenQueryingWithSQL_ThenDocumentsGetReturned()
        {
            // Arrange
            var cats = new List<Cat>();
            for (var i = 0; i < 5; i++)
            {
                var cat = new Cat { Name = $"Cat {i}" };
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(created.Entity);
            }

            // Act
            var results = (await _cosmonautClient.QueryDocumentsAsync<Document>(_databaseId, _collectionName,
                "select * from c where STARTSWITH(c.Name, @catName)", new { catName = "Cat" },
                new FeedOptions { EnableScanInQuery = true })).ToList();

            // Assert
            results.ForEach(result =>
            {
                cats.Select(x => x.CatId).Should().Contain(result.Id);
                cats.Select(cat => cat.Name).Should().Contain(result.GetPropertyValue<string>("Name"));
            });
        }

        [Fact]
        public async Task QueryDocumentsAsync_WhenQueryingAllDocuments_ThenAllDocumentsGetReturned()
        {
            // Arrange
            var cats = new List<Cat>();
            for (var i = 0; i < 5; i++)
            {
                var cat = new Cat { Name = $"Cat {i}" }.ConvertObjectToDocument();
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(JsonConvert.DeserializeObject<Cat>(created.Resource.ToString()));
            }

            // Act
            var results = (await _cosmonautClient.QueryDocumentsAsync(_databaseId, _collectionName)).ToList();

            // Assert
            results.ForEach(result =>
            {
                cats.Select(x => x.CatId).Should().Contain(result.Id);
                cats.Select(cat => cat.Name).Should().Contain(result.GetPropertyValue<string>("Name"));
            });
        }

        [Fact]
        public async Task WhenPaginatedQueryExecutesWithSkipTake_ThenPaginatedResultsAreReturnedCorrectly()
        {
            var cats = new List<Cat>();
            for (var i = 0; i < 15; i++)
            {
                var cat = new Cat { Name = $"Cat {i}" };
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(created.Entity);
            }
            cats = cats.OrderBy(x => x.Name).ToList();

            var firstPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x=>x.Name.StartsWith("Cat")).WithPagination(1, 5).OrderBy(x => x.Name).ToListAsync();
            var secondPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(2, 5).OrderBy(x => x.Name).ToListAsync();
            var thirdPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(3, 5).OrderBy(x => x.Name).ToListAsync();
            var fourthPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(4, 5).OrderBy(x => x.Name).ToListAsync();

            
            firstPage.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Take(5), ExcludeEtagCheck());
            secondPage.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Skip(5).Take(5), ExcludeEtagCheck());
            thirdPage.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Skip(10).Take(5), ExcludeEtagCheck());
            fourthPage.Should().BeEmpty();
        }

        [Fact]
        public async Task WhenPaginatedQueryExecutesWithNextPageAsync_ThenPaginatedResultsAreReturnedCorrectly()
        {
            var cats = new List<Cat>();
            for (var i = 0; i < 15; i++)
            {
                var cat = new Cat { Name = $"Cat {i}" };
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(created.Entity);
            }
            cats = cats.OrderBy(x => x.Name).ToList();

            var firstPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(1, 5).OrderBy(x => x.Name).ToPagedListAsync();
            var secondPage = await firstPage.GetNextPageAsync();
            var thirdPage = await secondPage.GetNextPageAsync();
            var fourthPage = await thirdPage.GetNextPageAsync();

            firstPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Take(5), ExcludeEtagCheck());
            secondPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Skip(5).Take(5), ExcludeEtagCheck());
            thirdPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Skip(10).Take(5), ExcludeEtagCheck());
            fourthPage.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task WhenPaginatedQueryAndFeedOptionsExecutesWithNextPageAsync_ThenPaginatedResultsAreReturnedCorrectly()
        {
            var cats = new List<Cat>();
            for (var i = 0; i < 15; i++)
            {
                var cat = new Cat { Name = $"Cat {i}" };
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(created.Entity);
            }
            cats = cats.OrderBy(x => x.Name).ToList();

            var firstPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName, new FeedOptions { RequestContinuation = "SomethingBad", MaxItemCount = 666 }).WithPagination(1, 5).OrderBy(x => x.Name).ToPagedListAsync();
            var secondPage = await firstPage.GetNextPageAsync();
            var thirdPage = await secondPage.GetNextPageAsync();
            var fourthPage = await thirdPage.GetNextPageAsync();

            firstPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Take(5), ExcludeEtagCheck());
            secondPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Skip(5).Take(5), ExcludeEtagCheck());
            thirdPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Skip(10).Take(5), ExcludeEtagCheck());
            fourthPage.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task WhenPaginatedQueryExecutesWithContinuationToken_ThenPaginatedResultsAreReturnedCorrectly()
        {
            var cats = new List<Cat>();
            for (var i = 0; i < 30; i++)
            {
                var cat = new Cat { Name = $"Cat {i}" };
                var created = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
                cats.Add(created.Entity);
            }
            cats = cats.OrderBy(x => x.Name).ToList();

            var firstPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(1, 10).OrderBy(x => x.Name).ToPagedListAsync();
            var secondPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(firstPage.NextPageToken, 10).OrderBy(x => x.Name).ToPagedListAsync();
            var thirdPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(secondPage.NextPageToken, 10).OrderBy(x => x.Name).ToPagedListAsync();
            var fourthPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(4, 10).OrderBy(x => x.Name).ToPagedListAsync();
            var emptyTokenPage = await _cosmonautClient.Query<Cat>(_databaseId, _collectionName).Where(x => x.Name.StartsWith("Cat")).WithPagination(string.Empty, 10).OrderBy(x => x.Name).ToPagedListAsync();

            firstPage.HasNextPage.Should().BeTrue();
            firstPage.NextPageToken.Should().NotBeNullOrEmpty();
            firstPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Take(10), ExcludeEtagCheck());
            secondPage.HasNextPage.Should().BeTrue();
            secondPage.NextPageToken.Should().NotBeNullOrEmpty();
            secondPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Skip(10).Take(10), ExcludeEtagCheck());
            thirdPage.HasNextPage.Should().BeFalse();
            thirdPage.NextPageToken.Should().BeNullOrEmpty();
            thirdPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Skip(20).Take(10), ExcludeEtagCheck());
            fourthPage.Results.Should().BeEmpty();
            fourthPage.NextPageToken.Should().BeNullOrEmpty();
            fourthPage.HasNextPage.Should().BeFalse();
            emptyTokenPage.HasNextPage.Should().BeTrue();
            emptyTokenPage.NextPageToken.Should().NotBeNullOrEmpty();
            emptyTokenPage.Results.Should().BeInAscendingOrder(x => x.Name).And.BeEquivalentTo(cats.Take(10), ExcludeEtagCheck());
        }

        [Fact]
        public async Task UpdateDocumentAsync_WhenUpdatingValidObject_ThenObjectGetUpdatedSuccessfully()
        {
            // Arrange
            var cat = new Cat { Name = "Kitty" };
            await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);

            // Act
            cat.Name = "MEGAKITTY";
            var updated = await _cosmonautClient.UpdateDocumentAsync(_databaseId, _collectionName, cat);

            // Assert
            updated.IsSuccess.Should().BeTrue();
            updated.Entity.Should().BeEquivalentTo(cat);
            updated.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            updated.ResourceResponse.Resource.GetPropertyValue<string>("Name").Should().Be("MEGAKITTY");
            updated.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
        }

        [Fact]
        public async Task UpdateDocumentAsync_WhenUpdatingValidDocument_ThenDocumentGetUpdatedSuccessfully()
        {
            // Arrange
            var cat = new Cat { Name = "Kitty" };
            await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);

            // Act
            cat.Name = "MEGAKITTY";
            var document = cat.ConvertObjectToDocument();
            var updated = await _cosmonautClient.UpdateDocumentAsync(_databaseId, _collectionName, document);

            // Assert
            updated.StatusCode.Should().Be(HttpStatusCode.OK);
            updated.Resource.GetPropertyValue<string>("Name").Should().Be("MEGAKITTY");
        }


        [Fact]
        public async Task UpsertDocumentAsync_WhenUpsertingValidObject_ThenObjectGetUpsertedSuccessfully()
        {
            // Arrange
            var cat = new Cat { Name = "Kitty" };
            await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);

            // Act
            cat.Name = "MEGAKITTY";
            var updated = await _cosmonautClient.UpsertDocumentAsync(_databaseId, _collectionName, cat);

            // Assert
            updated.IsSuccess.Should().BeTrue();
            updated.Entity.Should().BeEquivalentTo(cat);
            updated.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            updated.ResourceResponse.Resource.GetPropertyValue<string>("Name").Should().Be("MEGAKITTY");
            updated.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
        }

        [Fact]
        public async Task UpsertDocumentAsync_WhenUpsertingValidDocument_ThenDocumentGetUpsertedSuccessfully()
        {
            // Arrange
            var cat = new Cat { Name = "Kitty" };
            await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);

            // Act
            cat.Name = "MEGAKITTY";
            var document = cat.ConvertObjectToDocument();
            var updated = await _cosmonautClient.UpsertDocumentAsync(_databaseId, _collectionName, document);

            // Assert
            updated.StatusCode.Should().Be(HttpStatusCode.OK);
            updated.Resource.GetPropertyValue<string>("Name").Should().Be("MEGAKITTY");
        }
        
        [Fact]
        public async Task UpdateDocumentAsync_WhenUpdatingMissingObject_ThenOperationFails()
        {
            // Arrange
            var cat = new Cat { Name = "Kitty" };

            // Act
            cat.Name = "MEGAKITTY";
            var updated = await _cosmonautClient.UpdateDocumentAsync(_databaseId, _collectionName, cat);

            // Assert
            updated.IsSuccess.Should().BeFalse();
            updated.Entity.Should().BeEquivalentTo(cat);
            updated.CosmosOperationStatus.Should().Be(CosmosOperationStatus.ResourceNotFound);
            updated.ResourceResponse.Should().BeNull();
        }

        [Fact]
        public async Task UpdateDocumentAsync_WhenUpdatingMissingDocument_ThenOperationFails()
        {
            // Arrange
            var cat = new Cat { Name = "Kitty" };

            // Act
            cat.Name = "MEGAKITTY";
            var document = cat.ConvertObjectToDocument();
            var updated = await _cosmonautClient.UpdateDocumentAsync(_databaseId, _collectionName, document);

            // Assert
            updated.Should().BeNull();
        }


        [Fact]
        public async Task UpsertDocumentAsync_WhenUpsertingMissingObject_ThenOperationIsSuccessful()
        {
            // Arrange
            var cat = new Cat { Name = "Kitty" };

            // Act
            cat.Name = "MEGAKITTY";
            var updated = await _cosmonautClient.UpsertDocumentAsync(_databaseId, _collectionName, cat);

            // Assert
            updated.IsSuccess.Should().BeTrue();
            updated.Entity.Should().BeEquivalentTo(cat);
            updated.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            updated.ResourceResponse.Resource.GetPropertyValue<string>("Name").Should().Be("MEGAKITTY");
            updated.CosmosOperationStatus.Should().Be(CosmosOperationStatus.Success);
        }

        [Fact]
        public async Task UpsertDocumentAsync_WhenUpsertingMissingDocument_ThenOperationIsSuccessful()
        {
            // Arrange
            var cat = new Cat { Name = "Kitty" };

            // Act
            cat.Name = "MEGAKITTY";
            var document = cat.ConvertObjectToDocument();
            var updated = await _cosmonautClient.UpsertDocumentAsync(_databaseId, _collectionName, document);

            // Assert
            updated.StatusCode.Should().Be(HttpStatusCode.Created);
            updated.Resource.GetPropertyValue<string>("Name").Should().Be("MEGAKITTY");
        }

        [Fact]
        public async Task GetDocumentAsync_WhenDocumentExists_ThenReturnsDocument()
        {
            // Arrange
            var catId = Guid.NewGuid().ToString();
            var cat = new Cat { CatId = catId, Name = "Kitty" }.ConvertObjectToDocument();

            // Act
            var added = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
            var found = await _cosmonautClient.GetDocumentAsync(_databaseId, _collectionName, cat.Id);

            // Assert
            added.StatusCode.Should().Be(HttpStatusCode.Created);
            added.Resource.GetPropertyValue<string>("Name").Should().Be("Kitty");
            found.Should().NotBeNull();
            found.Id.Should().Be(cat.Id);
        }

        [Fact]
        public async Task GetDocumentAsync_WhenDocumentExists_ThenReturnsObject()
        {
            // Arrange
            var catId = Guid.NewGuid().ToString();
            var cat = new Cat { CatId = catId, Name = "Kitty" };

            // Act
            var added = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
            var found = await _cosmonautClient.GetDocumentAsync(_databaseId, _collectionName, cat.CatId);

            // Assert
            added.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            added.Entity.Name.Should().Be("Kitty");
            found.Should().NotBeNull();
            found.Id.Should().Be(cat.CatId);
        }

        [Fact]
        public async Task GetDocumentAsync_WhenDocumentDoesntExists_ThenReturnsNullObject()
        {
            // Arrange
            var catId = Guid.NewGuid().ToString();
            var cat = new Cat { CatId = catId, Name = "Kitty" };

            // Act
            var added = await _cosmonautClient.CreateDocumentAsync(_databaseId, _collectionName, cat);
            var found = await _cosmonautClient.GetDocumentAsync(_databaseId, _collectionName, cat.CatId);

            // Assert
            added.ResourceResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            added.Entity.Name.Should().Be("Kitty");
            found.Should().NotBeNull();
            found.Id.Should().Be(cat.CatId);
        }

        [Fact]
        public async Task GetDocumentAsync_WhenDocumentDoesntExists_ThenReturnsNoDocument()
        {
            // Arrange
            var catId = Guid.NewGuid().ToString();
            var cat = new Cat { CatId = catId, Name = "Kitty" }.ConvertObjectToDocument();

            // Act
            var found = await _cosmonautClient.GetDocumentAsync(_databaseId, _collectionName, cat.Id);

            // Assert
            found.Should().BeNull();
        }

        public void Dispose()
        {
            _cosmonautClient.DeleteCollectionAsync(_databaseId, _collectionName).GetAwaiter().GetResult();
            _cosmonautClient.DeleteDatabaseAsync(_databaseId).GetAwaiter().GetResult();
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