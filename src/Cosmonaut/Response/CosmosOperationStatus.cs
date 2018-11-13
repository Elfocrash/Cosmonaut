namespace Cosmonaut.Response
{
    internal enum CosmosOperationStatus
    {
        Success,
        RequestRateIsLarge,
        ResourceNotFound,
        PreconditionFailed,
        Conflict
    }
}