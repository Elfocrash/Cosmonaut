using System;
using System.Collections.Generic;
using System.Net;

namespace Cosmonaut.Diagnostics
{
    public class CosmosEventMetadata
    {
        public DateTimeOffset StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public string DependencyTypeName { get; set; }

        public string DependencyName { get; set; }

        public string Target { get; set; }

        public string Data { get; set; }

        public string ResultCode { get; set; } = HttpStatusCode.OK.ToString("D");

        public Exception Error { get; set; }

        public bool Success { get; set; }

        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}