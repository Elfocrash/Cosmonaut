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
            //Look at appsettings.Development.json
            var config = new AppSettingsSection();
            configuration.GetSection("CosmosDb").Bind(config);
            
            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionProtocol = Protocol.Tcp,
                ConnectionMode = ConnectionMode.Direct
            };
            
            var cosmosSettings = new CosmosStoreSettings(config.CosmosDatabaseName, 
                config.CosmosDatabaseUrl, 
                config.CosmosAuthKey
                , connectionPolicy
                , defaultCollectionThroughput: config.DefaultConnectionThroughput);
            
            AddCosmosStores(services, cosmosSettings);
        }

        private static void AddCosmosStores(IServiceCollection services, CosmosStoreSettings cosmosSettings)
        {
            services.AddCosmosStore<Person>(cosmosSettings);
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
}