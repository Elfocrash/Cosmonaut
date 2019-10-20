using System.Linq;
using System.Reflection;

namespace Cosmonaut.Extensions
{
    public static class CosmosQueryableExtensions
    {
        public static string GetContinuationToken<T>(this IQueryable<T> queryable)
        {
            if (!queryable.GetType().Name.Equals("CosmosLinqQuery`1"))
                return null;
            
            return queryable.GetType().GetField("continuationToken",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(queryable)?.ToString();
        }
        
        public static void SetContinuationToken<T>(this IQueryable<T> queryable, string continuationToken)
        {
            if (!queryable.GetType().Name.Equals("CosmosLinqQuery`1"))
                return;
            
            queryable.GetType().GetField("continuationToken",BindingFlags.Instance | BindingFlags.NonPublic).SetValue(queryable, continuationToken);
            queryable.Provider.GetType().GetField("continuationToken",BindingFlags.Instance | BindingFlags.NonPublic).SetValue(queryable.Provider, continuationToken);
        }
    }
}