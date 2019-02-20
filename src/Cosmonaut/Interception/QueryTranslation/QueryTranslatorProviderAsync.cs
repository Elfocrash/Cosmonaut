using Cosmonaut.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmonaut.Interception.QueryTranslation
{

    internal class QueryTranslatorProviderAsync : ExpressionVisitor, IQueryProvider
    {
        private static readonly TraceSource _traceSource = new TraceSource(typeof(QueryTranslatorProviderAsync).Name);
        private readonly IEnumerable<ExpressionVisitor> _visitors;

        internal IQueryable Source { get; }

        public IQueryProvider OriginalProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslatorProviderAsync"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="visitors">The visitors.</param>
        public QueryTranslatorProviderAsync(IQueryable source, IEnumerable<ExpressionVisitor> visitors)
        {
            Source = source;
            OriginalProvider = source.Provider;
            _visitors = visitors;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new QueryTranslator<TElement>(Source, expression, _visitors);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = expression.Type.GenericTypeArguments.First();
            return (IQueryable)Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType), Source, expression, _visitors);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var translated = VisitAllAndOptimize(expression);

            return Source.Provider.Execute<TResult>(translated);
        }

        public object Execute(Expression expression)
        {
            return Execute<object>(expression);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return ExecuteAsync<TResult>(expression, CancellationToken.None);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // In case Source.Provider is not a IDbAsyncQueryProvider or EntityQueryProvider, just start a new Task
            return Task.Factory.StartNew(() => Execute<TResult>(expression), cancellationToken);
        }

        public Task<object> ExecuteAsync(Expression expression)
        {
            return ExecuteAsync(expression, CancellationToken.None);
        }

        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            return ExecuteAsync<object>(expression, cancellationToken);
        }

        internal IEnumerable ExecuteEnumerable(Expression expression)
        {
            var translated = VisitAllAndOptimize(expression);

            return Source.Provider.CreateQuery(translated);
        }

        private Expression VisitAllAndOptimize(Expression expression)
        {
            // Run all visitors in order
            var visitors = new ExpressionVisitor[] { this }.Concat(_visitors);

            var translated = visitors.Aggregate(expression, (expr, visitor) => visitor.Visit(expr));

            return translated;
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ConstantExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Fix up the Expression tree to work with the underlying LINQ provider
            if (node.Type.GetTypeInfo().IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(QueryTranslator<>))
            {

                if (((IQueryable)node.Value).Provider is QueryTranslatorProviderAsync provider)
                {
                    return provider.Source.Expression;
                }

                return Source.Expression;
            }

            return base.VisitConstant(node);
        }
    }
}

