using System.Collections.Generic;
using Microsoft.Azure.Documents;

namespace Cosmonaut.StoredProcedures
{
    public class CosmonautStoredProcedures
    {
        public const string Version = "1_0_0";

        public static readonly StoredProcedure RemoveByExpression = new StoredProcedure
        {
            Id = $"cosmonautRemoveByExpression_{Version}",
            Body = Resources.cosmonautRemoveByExpression
        };

        public static IEnumerable<StoredProcedure> Values
        {
            get
            {
                yield return RemoveByExpression;
            }
        }
    }
}