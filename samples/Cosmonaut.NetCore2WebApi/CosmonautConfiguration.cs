using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Cosmonaut.NetCore2WebApi.Model;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmonaut.NetCore2WebApi
{
    public class CosmonautConfiguration
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            //Look at appsettings.Development.json | https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1
            var cosmosConfig = new AppSettingsSection();
            configuration.GetSection("CosmosDb").Bind(cosmosConfig);
            
            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionProtocol = Protocol.Tcp,
                ConnectionMode = ConnectionMode.Direct
            };
            
            var cosmosSettings = new CosmosStoreSettings(cosmosConfig.CosmosDatabaseName, 
                cosmosConfig.CosmosDatabaseUrl, 
                cosmosConfig.CosmosAuthKey
                , connectionPolicy
                , defaultCollectionThroughput: cosmosConfig.DefaultConnectionThroughput);
            
            AddCosmosStores(services, cosmosSettings);
        }

        private static void AddCosmosStores(IServiceCollection services, CosmosStoreSettings cosmosSettings)
        {
            services.AddCosmosStore<Person>(cosmosSettings);
        }
    }
    
    public class AppSettingsSection
    {
        public AppSettingsSection()
        {
            DefaultConnectionThroughput = 5000;
        }
            
        public string CosmosDatabaseName { get; set; }
        public string CosmosDatabaseUrl { get; set; }
        public string CosmosAuthKey { get; set; }
        public int DefaultConnectionThroughput { get; set; }
    }
}