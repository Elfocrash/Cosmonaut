using System.Collections.Generic;
using System.Linq;

namespace Cosmonaut.Response
{
    public class CosmosMultipleResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => _operationStatus == CosmosOperationStatus.Success && !FailedEntities.Any();

        public List<CosmosResponse<TEntity>> FailedEntities { get; } = new List<CosmosResponse<TEntity>>();

        private readonly CosmosOperationStatus _operationStatus = CosmosOperationStatus.Success;

        public CosmosMultipleResponse()
        {
            
        }

        public CosmosMultipleResponse(CosmosOperationStatus operationStatus)
        {
            _operationStatus = operationStatus;
        }

    }
}