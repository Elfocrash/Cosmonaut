using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Internal
{
    public static class InternalMethodInvocations
    {
        public static SqlQuerySpec EvaluateSqlQuery(this IQueryable queryable)
        {
            var expression = queryable.Expression;
            var sqlQuerySpec = (SqlQuerySpec)InternalTypeCache.Instance.DocumentQueryEvaluatorEvaluate.Invoke(expression, new object[]{ expression }) ??
                         new SqlQuerySpec("select * from root");

            var regex = new Regex("select(?s)(.*)from", RegexOptions.IgnoreCase);

            var matchedString = regex.Match(sqlQuerySpec.QueryText);

            if (matchedString.Groups.Count < 2)
                throw new Exception("Generated SQL cannot be parsed.");

            var firstGroup = matchedString.Groups[1].Value;
            var rootSymbol = CosmosSqlQueryExtensions.GetCollectionIdentifier(sqlQuerySpec.QueryText);
            sqlQuerySpec.QueryText = sqlQuerySpec.QueryText.Replace(firstGroup, $" {rootSymbol}._self ");
            return sqlQuerySpec;
        }
    }
}