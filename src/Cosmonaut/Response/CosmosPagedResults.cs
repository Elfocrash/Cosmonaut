using System.Collections.Generic;
using System.Linq;

namespace Cosmonaut.Response
{
    public class CosmosPagedResults<T>
    {
        internal CosmosPagedResults(List<T> results, string nextPageToken)
        {
            Results = results;
            NextPageToken = nextPageToken;
        }

        internal CosmosPagedResults(List<T> results, string nextPageToken, List<string> continuationTokens) : this(results, nextPageToken)
        {
            ContinuationTokens = continuationTokens;
        }

        public List<T> Results { get; }

        public string NextPageToken { get; }

        public List<string> ContinuationTokens { get; } = new List<string>();

        public bool HasNextPage => !string.IsNullOrEmpty(NextPageToken);

        public static implicit operator List<T>(CosmosPagedResults<T> results)
        {
            return results.Results.ToList();
        }
    }
}