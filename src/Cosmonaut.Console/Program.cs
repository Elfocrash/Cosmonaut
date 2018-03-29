using System;
using System.Collections.Generic;
using Cosmonaut.Extensions;
using Cosmonaut.Models;
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
            var books = new List<Book>();
            for (int i = 0; i < 50; i++)
            {
                books.Add(new Book
                {
                    CosmosId = Guid.NewGuid().ToString(),
                    Name = "Test " + i
                });
            }
            
            //cosmoStore.RemoveAsync(x => x.Name == "MYBOOK").GetAwaiter().GetResult();
            //var result = cosmoStore.WhereAsync(x => x.Author.Username == "nick").Result.ToList();
            //var results = cosmoStore.ToListAsync().Result;
            // System.Console.ReadKey();
        }
    }
}
