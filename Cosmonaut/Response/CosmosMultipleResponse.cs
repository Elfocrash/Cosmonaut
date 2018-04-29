using System.Collections.Generic;
using System.Linq;

namespace Cosmonaut.Response
{
    public class CosmosMultipleResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => _operationStatus == CosmosOperationStatus.Success && !FailedEntities.Any();

        public List<CosmosResponse<TEntity>> FailedEntities { get; } = new List<CosmosResponse<TEntity>>();

        public List<CosmosResponse<TEntity>> SuccessfulEntities { get; } = new List<CosmosResponse<TEntity>>();

        private readonly CosmosOperationStatus _operationStatus = CosmosOperationStatus.Success;

        public CosmosMultipleResponse()
        {
            
        }

        public CosmosMultipleResponse(CosmosOperationStatus operationStatus)
        {
            _operationStatus = operationStatus;
        }

        internal void AddResponse(CosmosResponse<TEntity> response)
        {
            if (response == null)
                return;

            if(response.IsSuccess)
                SuccessfulEntities.Add(response);
            else
                FailedEntities.Add(response);
        }
    }
}