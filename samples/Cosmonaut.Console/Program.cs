using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Cosmonaut.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cosmonaut.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Uncomment to enable Serilog logging
            //SerilogEventListener.Instance.Initialize();

            var jsonSerializerSettings = new CosmosJsonNetSerializer(new JsonSerializerSettings());
            
            var cosmosSettings = new CosmosStoreSettings("localtest", 
                "https://localhost:8081", 
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
                , ConnectionMode.Direct
                , defaultContainerThroughput: 5000);
            
            cosmosSettings.CosmosSerializer = jsonSerializerSettings;

            var cosmonautClient = new CosmonautClient("https://localhost:8081",
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

            var serviceCollection = new ServiceCollection();

//            serviceCollection.AddCosmosStore<Book>("localtest", "https://localhost:8081",
//                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
//                settings =>
//            {
//                settings.DefaultContainerThroughput = 5000;
//                settings.CosmosSerializer = jsonSerializerSettings;
//            });
            
            serviceCollection.AddCosmosStore<Car>(cosmosSettings);

            var provider = serviceCollection.BuildServiceProvider();

            //var booksStore = provider.GetService<ICosmosStore<Book>>();
            var carStore = provider.GetService<ICosmosStore<Car>>();
            
            System.Console.WriteLine($"Started");
            
//
//            var booksRemoved = await booksStore.RemoveAsync(x => true);
//            System.Console.WriteLine($"Removed {booksRemoved.SuccessfulEntities.Count} books from the database.");

            var carsRemoved = await carStore.RemoveAsync(x => true);
            System.Console.WriteLine($"Removed {carsRemoved.SuccessfulEntities.Count} cars from the database.");

            var books = new List<Book>();
            for (int i = 0; i < 25; i++)
            {
                books.Add(new Book
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test " + i,
                    AnotherRandomProp = "Random " + i
                });
            }

            var cars = new List<Car>();
            for (int i = 0; i < 25; i++)
            {
                cars.Add(new Car
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Car " + i,
                });
            }

            var watch = new Stopwatch();
            watch.Start();

            var addedCars = await carStore.AddRangeAsync(cars);
//
//            var addedBooks = await booksStore.AddRangeAsync(books);
//
//            System.Console.WriteLine($"Added {addedCars.SuccessfulEntities.Count + addedBooks.SuccessfulEntities.Count} documents in {watch.ElapsedMilliseconds}ms");
//            watch.Restart();
//            //await Task.Delay(3000);
//
//            var aCarId = addedCars.SuccessfulEntities.First().Entity.Id;
//
            var firstAddedCar = await carStore.Query().ToListAsync();
//            var allTheCars = await carStore.QueryMultipleAsync<Car>("select * from c");
//
//            var carPageOne = await carStore.Query("select * from c order by c.Name asc").WithPagination(1, 5).ToPagedListAsync();
//            var carPageTwo = await carStore.Query("select * from c order by c.Name asc").WithPagination(carPageOne.NextPageToken, 5).ToPagedListAsync();
//            var carPageThree = await carPageTwo.GetNextPageAsync();
//            var carPageFour = await carPageThree.GetNextPageAsync();
//
//            var addedRetrieved = await booksStore.Query().OrderBy(x=> x.Name).ToListAsync();
//
//            var firstPage = await booksStore.Query().WithPagination(1, 10).ToPagedListAsync();
//            var secondPage = await firstPage.GetNextPageAsync();
//            var thirdPage = await secondPage.GetNextPageAsync();
//            var fourthPage = await secondPage.GetNextPageAsync();
//
//            //var thirdPage = await booksStore.Query().WithPagination(secondPage.NextPageToken, 10).OrderBy(x => x.Name).ToPagedListAsync();
//            //var fourthPage = await booksStore.Query().WithPagination(4, 10).OrderBy(x => x.Name).ToListAsync();
//
//            var sqlPaged = await cosmonautClient.Query<Book>("localtest", "shared",
//                "select * from c where c.CosmosEntityName = @type order by c.Name", new Dictionary<string, object>{{ "type", "books" } }, new FeedOptions { EnableCrossPartitionQuery = true })
//                .WithPagination(2, 10).ToListAsync();
//
//            System.Console.WriteLine($"Retrieved {addedRetrieved.Count} documents in {watch.ElapsedMilliseconds}ms");
//            watch.Restart();
//            foreach (var addedre in addedRetrieved)
//            {
//                addedre.AnotherRandomProp += " Nick";
//            }
//
//            var updated = await booksStore.UpsertRangeAsync(addedRetrieved, x => new RequestOptions { AccessCondition = new AccessCondition
//            {
//                Type = AccessConditionType.IfMatch,
//                Condition = x.Etag
//            }});
//            System.Console.WriteLine($"Updated {updated.SuccessfulEntities.Count} documents in {watch.ElapsedMilliseconds}ms");
//            watch.Restart();
//
//            var removed = await booksStore.RemoveRangeAsync(addedRetrieved);
//            System.Console.WriteLine($"Removed {removed.SuccessfulEntities.Count} documents in {watch.ElapsedMilliseconds}ms");
//            watch.Reset();
//            watch.Stop();

            System.Console.ReadKey();
        }
    }
}
