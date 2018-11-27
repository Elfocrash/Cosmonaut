using System;
using System.Collections.Generic;
using Cosmonaut.WebJobs.Extensions.Trigger;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cosmonaut.WebJobs.Extensions.Config
{
    [Extension("CosmosStore")]
    internal class CosmosStoreExtensionConfigProvider<T> : IExtensionConfigProvider where T : class
    {
        private readonly IConfiguration _configuration;
        private readonly INameResolver _nameResolver;
        private readonly CosmosStoreBindingOptions _bindingOptions;
        private readonly ILoggerFactory _loggerFactory;

        public CosmosStoreExtensionConfigProvider(IOptions<CosmosStoreBindingOptions> options, IConfiguration configuration, INameResolver nameResolver, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _nameResolver = nameResolver;
            _bindingOptions = options.Value;
            _loggerFactory = loggerFactory;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var triggerRule = context.AddBindingRule<CosmosStoreTriggerAttribute>();
            triggerRule.BindToTrigger<IReadOnlyList<T>>(new CosmosStoreTriggerAttributeBindingProvider<T>(_configuration, _nameResolver, _bindingOptions, _loggerFactory));
            triggerRule.AddConverter<string, IReadOnlyList<T>>(JsonConvert.DeserializeObject<IReadOnlyList<T>>);
            triggerRule.AddConverter<IReadOnlyList<T>, JArray>(JArray.FromObject);
            triggerRule.AddConverter<IReadOnlyList<T>, string>(docList => JArray.FromObject(docList).ToString());
        }
        
    }
}