using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cosmonaut.Exceptions;

namespace Cosmonaut.Extensions
{
    internal static class CosmosSqlQueryExtensions
    {
        private static readonly Regex SingleIdentifierMatchRegex = new Regex("from ([a-zA-Z0-9]+)?", RegexOptions.IgnoreCase);
        private static readonly Regex IdentifierMatchRegex = new Regex("from ([a-zA-Z0-9]+)? ([a-zA-Z0-9]+)?", RegexOptions.IgnoreCase);
        private static readonly Regex IdentifierWithAsMatchRegex = new Regex("from ([a-zA-Z0-9]+)? as ([a-zA-Z0-9]+)?", RegexOptions.IgnoreCase);

        private static readonly IEnumerable<string> PostSelectCosmosSqlOperators = new[] {"where", "order", "join", "as", "select", "by"};

        internal static string EnsureQueryIsCollectionSharingFriendly<TEntity>(this string sql) where TEntity : class
        {
            var isSharedQuery = typeof(TEntity).UsesSharedCollection();

            if (!isSharedQuery)
                return sql;

            var identifier = GetCollectionIdentifier(sql);

            var cosmosEntityNameValue = $"{typeof(TEntity).GetSharedCollectionEntityName()}";

            var hasExistingWhereClause = sql.IndexOf(" where ", StringComparison.OrdinalIgnoreCase) >= 0;

            if (hasExistingWhereClause)
            {
                var splitQuery = sql.Split(new[] { " where " }, StringSplitOptions.None);
                var firstPartQuery = splitQuery[0];
                var secondPartQuery = splitQuery[1];
                var sharedCollectionExpressionQuery = $"{identifier}.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{cosmosEntityNameValue}'";
                return $"{firstPartQuery} where {sharedCollectionExpressionQuery} and {secondPartQuery}";
            }

            return $"{sql} where {identifier}.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{cosmosEntityNameValue}'";
        }

        private static string GetCollectionIdentifier(string sql)
        {
            var matchedWithAs = IdentifierWithAsMatchRegex.Match(sql);

            if (matchedWithAs.Success)
            {
                var potentialIdentifierFromAs = matchedWithAs.Groups[2].Value;
                if (PostSelectCosmosSqlOperators.Contains(potentialIdentifierFromAs, StringComparer.OrdinalIgnoreCase))
                {
                    throw new InvalidSqlQueryException(sql);
                }

                return potentialIdentifierFromAs;
            }

            var matchedGroups = IdentifierMatchRegex.Match(sql);
            if (matchedGroups.Success && matchedGroups.Groups.Count == 3)
            {
                var potentialIdentifier = matchedGroups.Groups[2].Value;

                if (PostSelectCosmosSqlOperators.Contains(potentialIdentifier, StringComparer.OrdinalIgnoreCase))
                {
                    return matchedGroups.Groups[1].Value;
                }

                return potentialIdentifier;
            }

            if (matchedGroups.Success && matchedGroups.Groups.Count == 2)
            {
                var potentialIdentifier = matchedGroups.Groups[1].Value;

                if (!PostSelectCosmosSqlOperators.Contains(potentialIdentifier, StringComparer.OrdinalIgnoreCase))
                {
                    return matchedGroups.Groups[1].Value;
                }
            }

            var singleIdentifierMatch = SingleIdentifierMatchRegex.Match(sql);

            if (singleIdentifierMatch.Success && !PostSelectCosmosSqlOperators.Contains(singleIdentifierMatch.Groups[1].Value))
            {
                return singleIdentifierMatch.Groups[1].Value;
            }

            throw new InvalidSqlQueryException(sql);
        }
    }
}