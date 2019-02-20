using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cosmonaut.Interception
{
    public class QueryInterceptor<TEntity> : ExpressionVisitor, IQueryInterceptor where TEntity : class
    {
        public Type Type { get; }

        public bool Applied { get; private set; }

        private readonly Expression<Func<TEntity, bool>> _predicate;
        private ConstantExpression _constant;

        public QueryInterceptor(Expression<Func<TEntity, bool>> interceptor)
        {
            if (interceptor == null)
            {
                throw new ArgumentException(nameof(interceptor));
            }

            Type = typeof(TEntity);
            _predicate = interceptor;
        }

        public override Expression Visit(Expression node)
        {
            if (!(node is ConstantExpression constant))
            {
                return base.Visit(node);
            }

            if (!(constant.Value is IQueryable<TEntity>))
            {
                return base.Visit(node);
            }

            var method = GetLinqWhere();

            Applied = true;

            return Expression.Call(method, constant, _predicate);
        }

        private MethodInfo GetLinqWhere()
        {
            var method = typeof(Queryable).GetMethods()
                .Where(x => x.Name == nameof(Queryable.Where))
                .Select(x => x.MakeGenericMethod(new[] { typeof(TEntity) }))
                .Single(methodInfo =>
                {
                    var parameters = methodInfo.GetParameters();

                    if (parameters.Count() == 2
                        && parameters[0].ParameterType == typeof(IQueryable<TEntity>)
                        && parameters[1].ParameterType == typeof(Expression<Func<TEntity, bool>>))
                    {
                        return true;
                    }

                    return false;
                });

            return method;
        }
    }
}