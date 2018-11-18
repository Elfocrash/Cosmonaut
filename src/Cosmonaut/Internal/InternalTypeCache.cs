using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Cosmonaut.Internal
{
    internal class InternalTypeCache
    {
        private static InternalTypeCache _instance;
        private static readonly object Padlock = new object();
        private const string LibVersion = ", Microsoft.Azure.DocumentDB.Core, Version=2.1.3.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        internal ConstructorInfo DocumentServiceResponseCtorInfo { get; }

        internal Type DictionaryNameValueCollectionType { get; }

        internal ConstructorInfo FeedResponseCtorInfo<T>() => Type.GetType($"Microsoft.Azure.Documents.Client.FeedResponse`1{LibVersion}").MakeGenericType(typeof(T))
            .GetTypeInfo().GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

        internal FieldInfo DocumentFeedOrDbLinkFieldInfo { get; }

        private ConcurrentDictionary<string, PropertyInfo[]> PropertyCache { get; } = new ConcurrentDictionary<string, PropertyInfo[]>();

        internal PropertyInfo[] GetPropertiesFromCache(Type type)
        {
            if(type == null)
                return new PropertyInfo[0];

            var entityName = type.AssemblyQualifiedName;

            if (PropertyCache.ContainsKey(entityName))
            {
                PropertyCache.TryGetValue(entityName, out var properties);
                return properties;
            }

            var props = type.GetTypeInfo().GetProperties();

            if (IsAnonymousType(type))
                return props;

            PropertyCache.TryAdd(entityName, props);
            return props;
        }

        private InternalTypeCache()
        {
            DocumentServiceResponseCtorInfo = Type.GetType($"Microsoft.Azure.Documents.DocumentServiceResponse{LibVersion}")
                .GetTypeInfo().GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            DictionaryNameValueCollectionType = Type.GetType($"Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection{LibVersion}");
            DocumentFeedOrDbLinkFieldInfo = Type.GetType($"Microsoft.Azure.Documents.Linq.DocumentQueryProvider{LibVersion}").GetTypeInfo()
                .GetField("documentsFeedOrDatabaseLink", BindingFlags.Instance | BindingFlags.NonPublic);
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

        private static bool IsAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Quick hack to detect anonymous types
            return type.Name.Contains("AnonymousType") && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"));
        }
    }
}