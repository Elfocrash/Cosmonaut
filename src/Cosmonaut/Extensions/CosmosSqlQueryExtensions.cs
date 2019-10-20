using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cosmonaut.Exceptions;
using Cosmonaut.Internal;

namespace Cosmonaut.Extensions
{
    internal static class CosmosSqlQueryExtensions
    {
        private static readonly Regex SingleIdentifierMatchRegex = new Regex("from ([a-zA-Z0-9]+)?", RegexOptions.IgnoreCase);
        private static readonly Regex IdentifierMatchRegex = new Regex("from ([a-zA-Z0-9]+)? ([a-zA-Z0-9]+)?", RegexOptions.IgnoreCase);
        private static readonly Regex IdentifierWithAsMatchRegex = new Regex("from ([a-zA-Z0-9]+)? as ([a-zA-Z0-9]+)?", RegexOptions.IgnoreCase);

        private static readonly IEnumerable<string> PostSelectCosmosSqlOperators = new[] {"where", "order", "join", "as", "select", "by"};

        internal static string EnsureQueryIsContainerSharingFriendly<TEntity>(this string sql) where TEntity : class
        {
            var isSharedQuery = typeof(TEntity).UsesSharedContainer();

            if (!isSharedQuery)
                return sql;

            var identifier = GetContainerIdentifier(sql);

            var cosmosEntityNameValue = $"{typeof(TEntity).GetSharedContainerEntityName()}";

            var hasExistingWhereClause = sql.IndexOf(" where ", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!hasExistingWhereClause)
            {
                var whereClause = $"where {identifier}.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{cosmosEntityNameValue}'";

                var hasOrderBy = sql.IndexOf(" order by ", StringComparison.OrdinalIgnoreCase) >= 0;

                if(!hasOrderBy)
                    return $"{sql} {whereClause}";

                var splitSql = Regex.Split(sql, " order by ", RegexOptions.IgnoreCase);

                return $"{splitSql[0]} {whereClause} order by {splitSql[1]}";
            }
            
            return GetQueryWithExistingWhereClauseInjectedWithSharedContainer(sql, identifier, cosmosEntityNameValue);
        }

        internal static IDictionary<string, object> ConvertToSqlParameterDictionary(this object obj)
        {
            var dictionary = new Dictionary<string, object>();

            if (obj == null)
                return dictionary;

            foreach (var propertyInfo in InternalTypeCache.Instance.GetPropertiesFromCache(obj.GetType()))
            {
                var propertyName = propertyInfo.Name.StartsWith("@") ? propertyInfo.Name : $"@{propertyInfo.Name}";
                var propertyValue = propertyInfo.GetValue(obj);
                dictionary.Add(propertyName, propertyValue);
            }

            return dictionary;
        }

        private static string GetQueryWithExistingWhereClauseInjectedWithSharedContainer(string sql,
            string identifier, string cosmosEntityNameValue)
        {
            var splitQuery = Regex.Split(sql, " where ", RegexOptions.IgnoreCase);
            var firstPartQuery = splitQuery[0];
            var secondPartQuery = splitQuery[1];
            var sharedContainerExpressionQuery =
                $"{identifier}.{nameof(ISharedCosmosEntity.CosmosEntityName)} = '{cosmosEntityNameValue}'";
            return $"{firstPartQuery} where {sharedContainerExpressionQuery} and {secondPartQuery}";
        }

        private static string GetContainerIdentifier(string sql)
        {
            var matchedWithAs = IdentifierWithAsMatchRegex.Match(sql);

            if (matchedWithAs.Success)
            {
                return GetPorentialIdentifierWithTheAsKeyword(sql, matchedWithAs);
            }

            var matchedGroups = IdentifierMatchRegex.Match(sql);
            if (matchedGroups.Success && matchedGroups.Groups.Count == 3)
            {
                return GetPotentialIdentifierWith3MatchedGroups(matchedGroups);
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

        private static string GetPotentialIdentifierWith3MatchedGroups(Match matchedGroups)
        {
            var potentialIdentifier = matchedGroups.Groups[2].Value;

            return PostSelectCosmosSqlOperators.Contains(potentialIdentifier, StringComparer.OrdinalIgnoreCase) ? matchedGroups.Groups[1].Value : potentialIdentifier;
        }

        private static string GetPorentialIdentifierWithTheAsKeyword(string sql, Match matchedWithAs)
        {
            var potentialIdentifierFromAs = matchedWithAs.Groups[2].Value;
            if (PostSelectCosmosSqlOperators.Contains(potentialIdentifierFromAs, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidSqlQueryException(sql);
            }

            return potentialIdentifierFromAs;
        }
    }
}