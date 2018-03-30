using System.Linq;
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
    }
}