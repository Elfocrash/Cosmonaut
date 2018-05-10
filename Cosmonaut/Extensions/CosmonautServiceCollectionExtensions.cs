using System;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmonaut.Extensions
{
    public static class CosmonautServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services, CosmosStoreSettings settings) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x=> new CosmosStore<TEntity>(settings));
            return services;
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services, Action<CosmosStoreSettings> settingsAction) where TEntity : class
        {
            var settings = new CosmosStoreSettings();
            settingsAction?.Invoke(settings);
            return services.AddCosmosStore<TEntity>(settings);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            IDocumentClient documentClient, 
            string databaseName,
            IDatabaseCreator databaseCreator = null,
            ICollectionCreator collectionCreator = null) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(documentClient, databaseName, databaseCreator, collectionCreator));
            return services;
        }
    }
}