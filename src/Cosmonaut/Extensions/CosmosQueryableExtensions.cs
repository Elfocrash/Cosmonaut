using System.Linq;
using System.Reflection;
using Cosmonaut.Internal;

namespace Cosmonaut.Extensions
{
    public static class CosmosQueryableExtensions
    {
        public static string GetContinuationToken<T>(this IQueryable<T> queryable)
        {
            if (!queryable.GetType().Name.Equals("CosmosLinqQuery`1"))
                return null;
            
            var queryableField = InternalTypeCache.Instance.GetFieldInfoFromCache(queryable.GetType(), "continuationToken", BindingFlags.Instance | BindingFlags.NonPublic);
            return queryableField.GetValue(queryable)?.ToString();
        }
        
        public static void SetContinuationToken<T>(this IQueryable<T> queryable, string continuationToken)
        {
            if (!queryable.GetType().Name.Equals("CosmosLinqQuery`1"))
                return;
            
            var queryableField = InternalTypeCache.Instance.GetFieldInfoFromCache(queryable.GetType(), "continuationToken", BindingFlags.Instance | BindingFlags.NonPublic);
            queryableField.SetValue(queryable, continuationToken);
            
            var queryableProviderField = InternalTypeCache.Instance.GetFieldInfoFromCache(queryable.Provider.GetType(), "continuationToken", BindingFlags.Instance | BindingFlags.NonPublic);
            queryableProviderField.SetValue(queryable.Provider, continuationToken);
        }
    }
}