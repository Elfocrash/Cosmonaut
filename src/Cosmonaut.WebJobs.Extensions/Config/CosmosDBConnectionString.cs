using System;
using System.Data.Common;

namespace Cosmonaut.WebJobs.Extensions.Config
{
    internal class CosmosDBConnectionString
    {
        public CosmosDBConnectionString(string connectionString)
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if (builder.TryGetValue("AccountKey", out object key))
            {
                AuthKey = key.ToString();
            }

            if (builder.TryGetValue("AccountEndpoint", out object uri))
            {
                ServiceEndpoint = new Uri(uri.ToString());
            }
        }

        public CosmosDBConnectionString(Uri endpoint, string authKey)
        {
            ServiceEndpoint = endpoint;
            AuthKey = authKey;
        }

        public Uri ServiceEndpoint { get; set; }

        public string AuthKey { get; set; }

        public override string ToString()
        {
            return $"AccountEndpoint={ServiceEndpoint};AccountKey={AuthKey}";
        }
    }
}
