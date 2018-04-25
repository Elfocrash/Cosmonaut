using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmonaut.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var newUser = new TestUser
            {
                Username = "nick",
                Id = Guid.NewGuid().ToString()
            };

            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionProtocol = Protocol.Tcp,
                ConnectionMode = ConnectionMode.Direct,
                RetryOptions = new RetryOptions
                {
                    MaxRetryAttemptsOnThrottledRequests = 2,
                    MaxRetryWaitTimeInSeconds = 3
                }
            };

            var cosmosSettings = new CosmosStoreSettings("localtest", 
                "https://localhost:8081", 
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
                , connectionPolicy
                , collectionThroughput: 5000
                , scaleCollectionRUsAutomatically: false);
           
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCosmosStore<Book>(cosmosSettings);

            var provider = serviceCollection.BuildServiceProvider();

            var cosmoStore = provider.GetService<ICosmosStore<Book>>();
            cosmoStore.RemoveAsync(x => true).GetAwaiter().GetResult();
            System.Console.WriteLine($"Started");
            
            var books = new List<Book>();
            for (int i = 0; i < 500; i++)
            {
                books.Add(new Book
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test " + i,
                    AnotherRandomProp = "Random " + i
                });
            }
            var watch = new Stopwatch();
            watch.Start();
            var added = cosmoStore.AddRangeAsync(books).Result;
            System.Console.WriteLine($"Added 1000 documents in {watch.ElapsedMilliseconds}ms");
            watch.Restart();

            var addedRetrieved = cosmoStore.ToListAsync().Result;
            System.Console.WriteLine($"Retrieved 1000 documents in {watch.ElapsedMilliseconds}ms");
            watch.Restart();
            foreach (var addedre in addedRetrieved)
            {
                addedre.AnotherRandomProp += " Nick";
            }

            var updated = cosmoStore.UpdateRangeAsync(addedRetrieved).Result;
            System.Console.WriteLine($"Updated 1000 documents in {watch.ElapsedMilliseconds}ms");
            watch.Restart();
            
            var removed = cosmoStore.RemoveRangeAsync(addedRetrieved).Result;
            System.Console.WriteLine($"Removed 1000 documents in {watch.ElapsedMilliseconds}ms");
            watch.Reset();
            watch.Stop();
            System.Console.ReadKey();
        }
    }
}
