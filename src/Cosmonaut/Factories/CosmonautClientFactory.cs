namespace Cosmonaut.Factories
{
    public class CosmonautClientFactory
    {
        public static ICosmonautClient CreateCosmonautClient(CosmosStoreSettings settings)
        {
            return new CosmonautClient(CosmosClientFactory.CreateCosmosClient(settings));
        }
    }
}