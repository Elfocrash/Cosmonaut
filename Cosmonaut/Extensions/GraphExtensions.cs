using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;

namespace Cosmonaut.Extensions
{
    public static class GraphExtensions
    {
        /// <summary>
        /// Method to create a Gremlin expression to create/query graph elements stored under an Azure CosmosDB collection.
        /// </summary>
        /// <typeparam name="T">Type of the object to query.</typeparam>
        /// <param name="documentClient">The IDocumentClient instance to execute the query.</param>
        /// <param name="collection">The DocumentCollection that contains the graph to query.</param>
        /// <param name="gremlinExpression">The Gremlin expression.</param>
        /// <param name="feedOptions">The options for processing the query result feed. For details <see cref="T:Microsoft.Azure.Documents.Client.FeedOptions" />.</param>
        /// <param name="graphSONMode">The GraphSON mode to use when returning the results of the query.</param>
        /// <returns>An <see cref="T:Microsoft.Azure.Documents.Linq.IDocumentQuery`1" /> that can evaluate the query.</returns>
        public static IDocumentQuery<T> CreateGremlinQuery<T>(this IDocumentClient documentClient, DocumentCollection collection, string gremlinExpression, FeedOptions feedOptions = null, GraphSONMode graphSONMode = GraphSONMode.Compact)
        {
            return Microsoft.Azure.Graphs.GraphExtensions.CreateGremlinQuery<T>((DocumentClient)documentClient, collection, gremlinExpression, feedOptions, graphSONMode);
        }

        /// <summary>
        /// Method to create a Gremlin expression to create/query graph elements stored under an Azure CosmosDB collection.
        /// </summary>
        /// <param name="documentClient">The IDocumentClient instance to execute the query.</param>
        /// <param name="collection">The DocumentCollection that contains the graph to query.</param>
        /// <param name="gremlinExpression">The Gremlin expression.</param>
        /// <param name="feedOptions">The options for processing the query result feed. For details <see cref="T:Microsoft.Azure.Documents.Client.FeedOptions" />.</param>
        /// <param name="graphSONMode">The GraphSON mode to use when returning the results of the query.</param>
        /// <returns>An <see cref="T:Microsoft.Azure.Documents.Linq.IDocumentQuery`1" /> that can evaluate the query.</returns>
        public static IDocumentQuery<object> CreateGremlinQuery(this IDocumentClient documentClient, DocumentCollection collection, string gremlinExpression, FeedOptions feedOptions = null, GraphSONMode graphSONMode = GraphSONMode.Compact)
        {
            return Microsoft.Azure.Graphs.GraphExtensions.CreateGremlinQuery<object>((DocumentClient)documentClient, collection, gremlinExpression, feedOptions, graphSONMode);
        }
    }
}