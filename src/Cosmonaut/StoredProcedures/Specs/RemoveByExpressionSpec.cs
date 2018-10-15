using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Cosmonaut.StoredProcedures.Specs
{
    public class RemoveByExpressionSpec
    {
        [JsonProperty("sqlExpression")]
        public string SqlExpression { get; }

        [JsonProperty("sqlParameterCollection")]
        public SqlParameterCollection SqlParameterCollection { get; }

        [JsonProperty("limit")]
        public int? Limit { get; }

        public RemoveByExpressionSpec(SqlQuerySpec sqlQuerySpec, int? limit = null)
        {
            SqlExpression = sqlQuerySpec?.QueryText;
            SqlParameterCollection = sqlQuerySpec?.Parameters;
            Limit = limit;
        }

        public RemoveByExpressionSpec(string sqlExpression, int? limit = null)
        {
            SqlExpression = sqlExpression;
            Limit = limit;
        }

        public RemoveByExpressionSpec(string sqlExpression, object parameters = null, int? limit = null)
        {
            SqlExpression = sqlExpression;
            SqlParameterCollection = parameters?.ConvertToSqlParameterCollection();
            Limit = limit;
        }

        public RemoveByExpressionSpec(string sqlExpression, SqlParameterCollection sqlParameterCollection, int? limit = null)
        {
            SqlExpression = sqlExpression;
            SqlParameterCollection = sqlParameterCollection;
            Limit = limit;
        }
    }
}