using System.Linq;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmonaut.Extensions
{
    public static class CosmonautServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services, CosmosStoreSettings settings) where TEntity : class
        {
            var documentClient = DocumentClientFactory.CreateDocumentClient(settings);
            services.AddSingleton<ICosmosStore<TEntity>>(x=> new CosmosStore<TEntity>(settings, 
                new CosmosDatabaseCreator(documentClient), 
                new CosmosCollectionCreator<TEntity>(documentClient)));
            return services;
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            IDocumentClient documentClient, 
            string databaseName,
            IDatabaseCreator databaseCreator,
            ICollectionCreator<TEntity> collectionCreator) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(documentClient, databaseName, databaseCreator, collectionCreator));
            return services;
        }
    }
}