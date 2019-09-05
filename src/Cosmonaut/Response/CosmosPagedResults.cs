using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Response
{
    public class CosmosPagedResults<T>
    {
        internal CosmosPagedResults(List<T> results, int pageSize, string nextPageToken)
        {
            Results = results;
            NextPageToken = nextPageToken;
            PageSize = pageSize;
        }

        internal CosmosPagedResults(List<T> results, int pageSize, string nextPageToken, FeedIterator<T> iterator)
        {
            Results = results;
            NextPageToken = nextPageToken;
            Iterator = iterator;
            PageSize = pageSize;
        }

        internal readonly int PageSize;

        internal readonly FeedIterator<T> Iterator;

        public List<T> Results { get; }

        public string NextPageToken { get; }

        public bool HasNextPage => !string.IsNullOrEmpty(NextPageToken);

//        public async Task<CosmosPagedResults<T>> GetNextPageAsync()
//        {
//            if(Iterator == null)
//                return new CosmosPagedResults<T>(new List<T>(), PageSize, string.Empty);
//
//            if(!HasNextPage)
//                return new CosmosPagedResults<T>(new List<T>(), PageSize, string.Empty);
//
//            if(PageSize <= 0)
//                return new CosmosPagedResults<T>(new List<T>(), PageSize, string.Empty);
//
//            return await Iterator.WithPagination(NextPageToken, PageSize).ToPagedListAsync();
//        }
        
        public static implicit operator List<T>(CosmosPagedResults<T> results)
        {
            return results.Results;
        }
    }
}