using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Response
{
    public class CosmosMultipleResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => !FailedEntities.Any();

        public List<ItemResponse<TEntity>> FailedEntities { get; } = new List<ItemResponse<TEntity>>();

        public List<ItemResponse<TEntity>> SuccessfulEntities { get; } = new List<ItemResponse<TEntity>>();

        internal void AddResponse(ItemResponse<TEntity> response)
        {
            if (response == null)
                return;

            if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 400) //TODO check this
            {
                SuccessfulEntities.Add(response);
                return;
            }

            FailedEntities.Add(response);
        }
    }
}