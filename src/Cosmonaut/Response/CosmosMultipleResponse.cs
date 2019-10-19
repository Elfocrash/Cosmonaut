using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Response
{
    public class CosmosMultipleResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => !FailedEntities.Any();

        public List<ResponseMessage> FailedEntities { get; } = new List<ResponseMessage>();

        public List<ResponseMessage> SuccessfulEntities { get; } = new List<ResponseMessage>();

        internal void AddResponse(ResponseMessage response)
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