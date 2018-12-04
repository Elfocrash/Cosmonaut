using System.Collections.Generic;
using System.Linq;

namespace Cosmonaut.Response
{
    public class CosmosMultipleResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => !FailedEntities.Any();

        public List<CosmosResponse<TEntity>> FailedEntities { get; } = new List<CosmosResponse<TEntity>>();

        public List<CosmosResponse<TEntity>> SuccessfulEntities { get; } = new List<CosmosResponse<TEntity>>();

        internal void AddResponse(CosmosResponse<TEntity> response)
        {
            if (response == null)
                return;

            if (response.IsSuccess)
            {
                SuccessfulEntities.Add(response);
                return;
            }

            FailedEntities.Add(response);
        }
    }
}