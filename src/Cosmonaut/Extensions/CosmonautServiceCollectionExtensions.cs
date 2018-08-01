using System;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmonaut.Extensions
{
    public static class CosmonautServiceCollectionExtensions
    {
        public static IServiceCollection  AddCosmosStore<TEntity>(this IServiceCollection services, CosmosStoreSettings settings) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x=> new CosmosStore<TEntity>(settings));
            return services;
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services, CosmosStoreSettings settings, string overriddenCollectionName) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(settings, overriddenCollectionName));
            return services;
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services, Action<CosmosStoreSettings> settingsAction) where TEntity : class
        {
            var settings = new CosmosStoreSettings();
            settingsAction?.Invoke(settings);
            return services.AddCosmosStore<TEntity>(settings);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services, Action<CosmosStoreSettings> settingsAction, string overriddenCollectionName) where TEntity : class
        {
            var settings = new CosmosStoreSettings();
            settingsAction?.Invoke(settings);
            return services.AddCosmosStore<TEntity>(settings, overriddenCollectionName);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            ICosmonautClient cosmonautClient,
            string databaseName,
            string authKey,
            string endpoint) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(cosmonautClient, databaseName, authKey, endpoint));
            return services;
        }
        
        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            ICosmonautClient cosmonautClient,
            string databaseName,
            string authKey,
            string endpoint,
            string overriddenCollectionName) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(cosmonautClient, databaseName, authKey, endpoint, overriddenCollectionName));
            return services;
        }
    }
}  