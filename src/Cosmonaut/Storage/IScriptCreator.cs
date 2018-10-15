using System.Threading.Tasks;

namespace Cosmonaut.Storage
{
    public interface IScriptCreator
    {
        Task<bool> EnsureCreatedAsync(string databaseId, string collectionId);
    }
}