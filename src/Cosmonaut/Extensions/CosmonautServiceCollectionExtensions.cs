using System;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmonaut.Extensions
{
    public static class CosmonautServiceCollectionExtensions
    {
        public static IServiceCollection  AddCosmosStore<TEntity>(this IServiceCollection services, CosmosStoreSettings settings) where TEntity : class
        {
            services.AddCosmosStore<TEntity>(settings, string.Empty);
            return services;
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services, CosmosStoreSettings settings, string overriddenCollectionName) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(settings, overriddenCollectionName));
            return services;
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            string databaseName, string endpointUri, string authKey,
            Action<CosmosStoreSettings> settingsAction) where TEntity : class
        {
            return services.AddCosmosStore<TEntity>(databaseName, new Uri(endpointUri), databaseName, settingsAction, string.Empty);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            string databaseName, string endpointUri, string authKey,
            Action<CosmosStoreSettings> settingsAction, string overriddenCollectionName) where TEntity : class
        {
            return services.AddCosmosStore<TEntity>(databaseName, new Uri(endpointUri), authKey, settingsAction, overriddenCollectionName);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            string databaseName, Uri endpointUri, string authKey,
            Action<CosmosStoreSettings> settingsAction) where TEntity : class
        {
            return services.AddCosmosStore<TEntity>(databaseName, endpointUri, databaseName, settingsAction, string.Empty);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            string databaseName, Uri endpointUri, string authKey,
            Action<CosmosStoreSettings> settingsAction, string overriddenCollectionName) where TEntity : class
        {
            var settings = new CosmosStoreSettings(databaseName, endpointUri, authKey);
            settingsAction?.Invoke(settings);
            return services.AddCosmosStore<TEntity>(settings, overriddenCollectionName);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            ICosmonautClient cosmonautClient,
            string databaseName) where TEntity : class
        {
            services.AddCosmosStore<TEntity>(cosmonautClient, databaseName, string.Empty);
            return services;
        }
        
        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            ICosmonautClient cosmonautClient,
            string databaseName,
            string overriddenCollectionName) where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(cosmonautClient, databaseName, overriddenCollectionName));
            return services;
        }
    }
}  