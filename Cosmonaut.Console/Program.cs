using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cosmonaut.Extensions;
using Cosmonaut.Models;
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

            var book = new Book
            {
                Name = "MYBOOK",
                Author = newUser
            };

            var cosmosSettings = new CosmosStoreSettings("localtest", 
                new Uri("https://localhost:8081"), 
                "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", collectionThroughput: 600);
           
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCosmosStore<Book>(cosmosSettings);

            var provider = serviceCollection.BuildServiceProvider();

            var cosmoStore = provider.GetService<ICosmosStore<Book>>();
            cosmoStore.RemoveAsync(x => true).GetAwaiter().GetResult();
            System.Console.WriteLine($"Started");
            
            var books = new List<Book>();
            for (int i = 0; i < 1000; i++)
            {
                books.Add(new Book
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test " + i
                });
            }
            var watch = new Stopwatch();
            watch.Start();
            var added = cosmoStore.AddAsync(book).Result;
            System.Console.WriteLine($"Added 1 document in {watch.ElapsedMilliseconds}ms");
            watch.Restart();
            var result = cosmoStore.FirstOrDefaultAsync(x => x.Name == "MYBOOK").Result;
            System.Console.WriteLine($"Retrieved 1 document in {watch.ElapsedMilliseconds}ms");
            watch.Restart();
            result.Name = "test";
            var updated = cosmoStore.UpdateAsync(result).Result;
            System.Console.WriteLine($"Updated 1 document in {watch.ElapsedMilliseconds}ms");
            watch.Restart();
            var removed = cosmoStore.RemoveRangeAsync(updated.Entity).Result;
            System.Console.WriteLine($"Removed 1 document in {watch.ElapsedMilliseconds}ms");
            watch.Reset();
            watch.Stop();
            System.Console.ReadKey();
            //cosmoStore.RemoveAsync(x => x.Name == "MYBOOK").GetAwaiter().GetResult();
            //var result = cosmoStore.WhereAsync(x => x.Author.Username == "nick").Result.ToList();
            //var results = cosmoStore.ToListAsync().Result;
            // System.Console.ReadKey();
        }
    }
}
