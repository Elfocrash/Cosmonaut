using System.Linq;
using Microsoft.Azure.Documents.Linq;

namespace Cosmonaut.Unit
{
    public interface IFakeDocumentQuery<T> : IDocumentQuery<T>, IOrderedQueryable<T>
    {

    }
}