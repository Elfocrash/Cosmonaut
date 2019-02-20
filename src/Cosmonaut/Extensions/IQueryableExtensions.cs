using Cosmonaut.Interception.QueryTranslation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Cosmonaut.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TEntity> ApplyInterception<TEntity>(this IQueryable<TEntity> queryable, IEnumerable<IQueryInterceptor> interceptors)
        {
            interceptors = interceptors?.Where(x => x.Type == typeof(TEntity));

            if (!interceptors?.Any() ?? true)
            {
                return queryable;
            }

            var visitors = interceptors.Cast<ExpressionVisitor>();
            return queryable.InterceptWith(visitors.ToArray());
        }

        public static IQueryable<T> InterceptWith<T>(this IQueryable<T> source, params ExpressionVisitor[] visitors)
        {
            return new QueryTranslator<T>(source, visitors);
        }
    }
}

