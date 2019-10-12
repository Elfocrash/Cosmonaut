using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Extensions;

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

        internal CosmosPagedResults(List<T> results, int pageSize, string nextPageToken, IQueryable<T> queryable)
        {
            Results = results;
            NextPageToken = nextPageToken;
            Queryable = queryable;
            PageSize = pageSize;
        }

        internal readonly int PageSize;

        internal readonly IQueryable<T> Queryable;

        public List<T> Results { get; }

        public string NextPageToken { get; }

        [Obsolete("Cosmos changed the way it used to work so this isn't accurate any more. You can now " +
                  "get a continuation token but the next result will be empty and have no token.")]
        public bool HasNextPage => !string.IsNullOrEmpty(NextPageToken);

        public async Task<CosmosPagedResults<T>> GetNextPageAsync()
        {
            if(Queryable == null)
                return new CosmosPagedResults<T>(new List<T>(), PageSize, string.Empty);

            if(!HasNextPage)
                return new CosmosPagedResults<T>(new List<T>(), PageSize, string.Empty);

            if(PageSize <= 0)
                return new CosmosPagedResults<T>(new List<T>(), PageSize, string.Empty);

            return await Queryable.WithPagination(NextPageToken, PageSize).ToPagedListAsync();
        }
        
        public static implicit operator List<T>(CosmosPagedResults<T> results)
        {
            return results.Results;
        }
    }
}