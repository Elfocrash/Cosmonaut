using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cosmonaut.Tests
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void AddCosmosStoreRegistersStore()
        {
            // Assign
            var serviceCollection = new ServiceCollection();
            var documentClient = MockHelpers.GetFakeDocumentClient();
            
            //Act
            serviceCollection.AddCosmosStore<Dummy>(documentClient.Object, "databaseName");
            var provider = serviceCollection.BuildServiceProvider();

            //Assert
            var cosmosStore = provider.GetService<ICosmosStore<Dummy>>();
            Assert.NotNull(cosmosStore);
        }
    }
}