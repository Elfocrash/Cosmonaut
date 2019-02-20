using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cosmonaut
{
    public class CosmosStoreSettings
    {
        public string DatabaseName { get; }

        internal string AuthKey { get; }

        public Uri EndpointUrl { get; }

        public ConnectionPolicy ConnectionPolicy { get; set; }

        public ConsistencyLevel? ConsistencyLevel { get; set; } = null;

        public IndexingPolicy IndexingPolicy { get; set; } = CosmosConstants.DefaultIndexingPolicy;

        public int DefaultCollectionThroughput { get; set; } = CosmosConstants.MinimumCosmosThroughput;

        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public bool InfiniteRetries { get; set; } = true;

        public string CollectionPrefix { get; set; } = string.Empty;

        public List<IQueryInterceptor> Interceptors { get; }

        public CosmosStoreSettings(string databaseName,
            string endpointUrl,
            string authKey,
            Action<CosmosStoreSettings> settings) : this(databaseName, new Uri(endpointUrl), authKey, settings)
        {
        }

        public CosmosStoreSettings(string databaseName,
            Uri endpointUrl,
            string authKey,
            Action<CosmosStoreSettings> settings)
        {
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            EndpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
            AuthKey = authKey ?? throw new ArgumentNullException(nameof(authKey));
            Interceptors = new List<IQueryInterceptor>();
            settings?.Invoke(this);
        }

        public CosmosStoreSettings(
            string databaseName,
            string endpointUrl,
            string authKey,
            ConnectionPolicy connectionPolicy = null,
            IndexingPolicy indexingPolicy = null,
            int defaultCollectionThroughput = CosmosConstants.MinimumCosmosThroughput)
            : this(databaseName,
                  new Uri(endpointUrl),
                  authKey,
                  connectionPolicy,
                  indexingPolicy,
                  defaultCollectionThroughput)
        {
        }

        public CosmosStoreSettings(
            string databaseName,
            Uri endpointUrl,
            string authKey,
            ConnectionPolicy connectionPolicy = null,
            IndexingPolicy indexingPolicy = null,
            int defaultCollectionThroughput = CosmosConstants.MinimumCosmosThroughput)
        {
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            EndpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
            AuthKey = authKey ?? throw new ArgumentNullException(nameof(authKey));
            ConnectionPolicy = connectionPolicy;
            DefaultCollectionThroughput = defaultCollectionThroughput;
            IndexingPolicy = indexingPolicy ?? CosmosConstants.DefaultIndexingPolicy;
        }

        public void AddInterceptor<T>(Expression<Func<T, bool>> filter) where T : class
        {
            Interceptors.Add(new QueryInterceptor<T>(filter));
        }
    }

    public interface IQueryInterceptor
    {
        Type Type { get; }
    }

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