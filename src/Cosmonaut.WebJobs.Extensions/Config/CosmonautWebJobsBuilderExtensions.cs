using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmonaut.WebJobs.Extensions.Config
{
    public static class CosmonautWebJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddCosmosStoreBinding<T>(this IWebJobsBuilder builder) where T : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            
            builder.AddExtension<CosmosStoreExtensionConfigProvider<T>>()               
                .ConfigureOptions<CosmosStoreBindingOptions>((config, path, options) =>
                {
                    options.ConnectionString = config.GetConnectionString(Constants.DefaultConnectionStringName);
                    IConfigurationSection section = config.GetSection(path);
                    section.Bind(options);
                });                
            
            return builder;
        }

        public static IWebJobsBuilder AddCosmosStoreBinding<T>(this IWebJobsBuilder builder, Action<CosmosStoreBindingOptions> configure) where T : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddCosmosStoreBinding<T>();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}