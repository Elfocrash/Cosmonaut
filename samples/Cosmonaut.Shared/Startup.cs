using Cosmonaut.Shared;
using Cosmonaut.WebJobs.Extensions.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(Startup))]
namespace Cosmonaut.Shared
{

    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            //builder.AddCosmosStoreBinding<Entity>();
            //builder.AddCosmosStoreBinding<Alpaca>();
            builder.AddCosmosStoreBinding<Llama>();
        }
    }
}