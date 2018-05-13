using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;

namespace Cosmonaut
{
    public interface IGraphStore
    {
        /// <summary>
        ///     Exposes the lower level DocumentClient
        /// </summary>
        IDocumentClient DocumentClient { get; }

        Task<T> QuerySingleAsync<T>(string gremlinExpression, FeedOptions feedOptions = null,
            GraphSONMode graphSONMode = GraphSONMode.Compact, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryMultipleAsync<T>(string gremlinExpression, FeedOptions feedOptions = null,
            GraphSONMode graphSONMode = GraphSONMode.Compact, CancellationToken cancellationToken = default);

        Task ExecuteAsync(string gremlinExpression, FeedOptions feedOptions = null,
            GraphSONMode graphSONMode = GraphSONMode.Compact, CancellationToken cancellationToken = default);

        Task<T> ExecuteSingleAsync<T>(string gremlinExpression, FeedOptions feedOptions = null,
            GraphSONMode graphSONMode = GraphSONMode.Compact, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> ExecuteAsync<T>(string gremlinExpression, FeedOptions feedOptions = null,
            GraphSONMode graphSONMode = GraphSONMode.Compact, CancellationToken cancellationToken = default);
    }
}