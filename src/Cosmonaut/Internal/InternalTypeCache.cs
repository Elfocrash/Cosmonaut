using System;
using System.Reflection;

namespace Cosmonaut.Internal
{
    internal class InternalTypeCache
    {
        private static InternalTypeCache _instance;
        private static readonly object Padlock = new object();
        private const string LibVersion = ", Microsoft.Azure.DocumentDB.Core, Version=2.1.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        internal ConstructorInfo DocumentServiceResponseCtorInfo { get; }

        internal Type DictionaryNameValueCollectionType { get; }

        internal ConstructorInfo FeedResponseCtorInfo<T>() => Type.GetType($"Microsoft.Azure.Documents.Client.FeedResponse`1{LibVersion}").MakeGenericType(typeof(T))
            .GetTypeInfo().GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

        internal MethodInfo DocumentQueryEvaluatorEvaluate { get; }

        internal Type DocumentQueryType { get; }

        private InternalTypeCache()
        {
            DocumentServiceResponseCtorInfo = Type.GetType($"Microsoft.Azure.Documents.DocumentServiceResponse{LibVersion}")
                .GetTypeInfo().GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            DictionaryNameValueCollectionType = Type.GetType($"Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection{LibVersion}");
            DocumentQueryEvaluatorEvaluate = Type.GetType($"Microsoft.Azure.Documents.Linq.DocumentQueryEvaluator{LibVersion}").GetTypeInfo().GetMethod("Evaluate");
            DocumentQueryType = Type.GetType($"Microsoft.Azure.Documents.Linq.DocumentQuery`1{LibVersion}");
        }

        internal static InternalTypeCache Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new InternalTypeCache());
                }
            }
        }
    }
}