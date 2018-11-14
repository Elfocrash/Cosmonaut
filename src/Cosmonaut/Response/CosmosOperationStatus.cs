namespace Cosmonaut.Response
{
    public enum CosmosOperationStatus
    {
        Success,
        RequestRateIsLarge,
        ResourceNotFound,
        PreconditionFailed,
        Conflict
    }
}