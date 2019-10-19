using System;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmonaut.Response
{
    public class CosmosResponse<TEntity> where TEntity : class
    {
        public bool IsSuccess => ResponseMessage.IsSuccessStatusCode;

        public CosmosOperationStatus CosmosOperationStatus { get; } = CosmosOperationStatus.Success;

        public ResponseMessage ResponseMessage { get; }

        public Lazy<TEntity> Entity { get; }

        public Exception Exception { get; }

        public CosmosResponse(CosmosSerializer cosmosSerializer, ResponseMessage responseMessage)
        {
            ResponseMessage = responseMessage;
            Entity = new Lazy<TEntity>(() => cosmosSerializer.FromStream<TEntity>(ResponseMessage.Content));
            //CosmosOperationStatus TODO Map this here 
        }
//
//        public CosmosResponse(TEntity entity, ResponseMessage responseMessage)
//        {
//            ResponseMessage = responseMessage;
//            Entity = entity;
//        }
//
//        public CosmosResponse(TEntity entity, Exception exception, CosmosOperationStatus statusType)
//        {
//            CosmosOperationStatus = statusType;
//            Entity = entity;
//            Exception = exception;
//        }
    }
}