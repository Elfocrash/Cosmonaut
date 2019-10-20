using System;
using System.Net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmonaut.Response
{
    public class CosmosResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => ResponseMessage.IsSuccessStatusCode;

        public CosmosOperationStatus CosmosOperationStatus { get; }

        public ResponseMessage ResponseMessage { get; }

        public Lazy<TEntity> Entity { get; }

        public string ErrorMessage => ResponseMessage.ErrorMessage;

        public CosmosResponse(CosmosSerializer cosmosSerializer, ResponseMessage responseMessage)
        {
            ResponseMessage = responseMessage;
            Entity = new Lazy<TEntity>(() => cosmosSerializer.FromStream<TEntity>(ResponseMessage.Content));
            CosmosOperationStatus = GetCosmosOperationStatus(responseMessage);
        }

        private CosmosOperationStatus GetCosmosOperationStatus(ResponseMessage responseMessage)
        {
            switch ((int)responseMessage.StatusCode)
            {
                case 429:
                    return CosmosOperationStatus.RequestRateIsLarge;
                case 404:
                    return CosmosOperationStatus.ResourceNotFound;
                case 412:
                    return CosmosOperationStatus.PreconditionFailed;
                case 409:
                    return CosmosOperationStatus.Conflict;
                default:
                    return CosmosOperationStatus.Success;
            }
        }
    }
}