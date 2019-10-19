using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut
{
    public interface ICosmonautClient
    {
        CosmosClient CosmosClient { get; }
    }
}