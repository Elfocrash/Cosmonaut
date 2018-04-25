namespace Cosmonaut.Response
{
    public enum CosmosOperationStatus
    {
        Success,
        ResourceNotFound,
        ResourceWithIdAlreadyExists,
        RequestRateIsLarge,
        GeneralFailure
    }
}