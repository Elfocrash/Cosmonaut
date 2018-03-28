using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var databaseName = "localtest";

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

            var documentClient = new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

            var cosmoStore = new CosmoStore<Book>(documentClient, databaseName);

            //for(int i = 0; i < 10; i++)
            //cosmoStore.AddAsync(book).GetAwaiter().GetResult();
            //var result = cosmoStore.FirstOrDefaultAsync(x => x.Name == "MYBOOK").Result;

            //result.Name = "yourbook";
            var updated = cosmoStore.UpdateAsync(book).Result;
            //var selectedEntity = cosmoStore.FirstOrDefaultAsync(x => x.CosmosId == "0a65e8c9-2c9d-4a04-a515-5f945af0c40a").Result;
            //var result = cosmoStore.RemoveByIdAsync("a36ad690-4719-43a3-95ad-7d59954544ca").GetAwaiter().GetResult();
            //var result = cosmoStore.WhereAsync(x => x.Author.Username == "nick").Result.ToList();
            //var results = cosmoStore.ToListAsync().Result;
            // System.Console.ReadKey();
        }
    }
}
