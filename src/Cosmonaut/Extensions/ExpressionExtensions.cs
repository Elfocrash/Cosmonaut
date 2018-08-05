using System;
using System.Linq.Expressions;

namespace Cosmonaut.Extensions
{
    internal static class ExpressionExtensions
    {
        internal static Expression<Func<TEntity, bool>> SharedCollectionExpression<TEntity>() where TEntity : class
        {
            var parameter = Expression.Parameter(typeof(ISharedCosmosEntity));
            var member = Expression.Property(parameter, nameof(ISharedCosmosEntity.CosmosEntityName));
            var contant = Expression.Constant(typeof(TEntity).GetSharedCollectionEntityName());
            var body = Expression.Equal(member, contant);
            var extra = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
            return extra;
        }
    }
}