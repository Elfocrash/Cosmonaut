using System;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmonaut.Extensions.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services, CosmosStoreSettings settings, string overriddenContainerName = "") where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(settings, overriddenContainerName));
            return services;
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            string databaseName, string endpointUri, string authKey,
            Action<CosmosStoreSettings> settingsAction = null, string overriddenContainerName = "") where TEntity : class
        {
            return services.AddCosmosStore<TEntity>(databaseName, new Uri(endpointUri), authKey, settingsAction, overriddenContainerName);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            string databaseName, Uri endpointUri, string authKey, Action<CosmosStoreSettings> settingsAction = null,
            string overriddenContainerName = "") where TEntity : class
        {
            var settings = new CosmosStoreSettings(databaseName, endpointUri, authKey);
            settingsAction?.Invoke(settings);
            return services.AddCosmosStore<TEntity>(settings, overriddenContainerName);
        }

        public static IServiceCollection AddCosmosStore<TEntity>(this IServiceCollection services,
            ICosmonautClient cosmonautClient,
            string databaseName,
            string overriddenContainerName = "") where TEntity : class
        {
            services.AddSingleton<ICosmosStore<TEntity>>(x => new CosmosStore<TEntity>(cosmonautClient, databaseName, overriddenContainerName));
            return services;
        }
    }
}  