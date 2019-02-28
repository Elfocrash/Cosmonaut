using System.Threading.Tasks;

namespace Cosmonaut.Storage
{
    public interface IDatabaseCreator
    {
        Task<bool> EnsureCreatedAsync(string databaseName, int? databaseThroughput = null);
    }
}