using System;
using System.Linq.Expressions;

namespace Cosmonaut.Extensions
{
    internal static class ExpressionExtensions
    {
        internal static Expression<Func<T, bool>> AndAlso<T>(this 
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left ?? throw new InvalidOperationException(), right ?? throw new InvalidOperationException()), parameter);
        }

        internal static Expression<Func<TEntity, bool>> SharedCollectionExpression<TEntity>() where TEntity : class
        {
            var parameter = Expression.Parameter(typeof(ISharedCosmosEntity));
            var member = Expression.Property(parameter, nameof(ISharedCosmosEntity.CosmosEntityName));
            var contant = Expression.Constant(typeof(TEntity).GetSharedCollectionEntityName());
            var body = Expression.Equal(member, contant);
            var extra = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
            return extra;
        }

        private class ReplaceExpressionVisitor
            : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}