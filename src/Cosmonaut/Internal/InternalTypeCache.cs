using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Cosmonaut.Internal
{
    internal class InternalTypeCache
    {
        private static InternalTypeCache _instance;
        private static readonly object Padlock = new object();
        private const string LibVersion = ", Microsoft.Azure.DocumentDB.Core, Version=2.8.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        internal ConstructorInfo DocumentServiceResponseCtorInfo { get; }

        internal Type DictionaryNameValueCollectionType { get; }

        internal ConstructorInfo FeedResponseCtorInfo<T>() => Type.GetType($"Microsoft.Azure.Documents.Client.FeedResponse`1{LibVersion}").MakeGenericType(typeof(T))
            .GetTypeInfo().GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

        internal FieldInfo DocumentFeedOrDbLinkFieldInfo { get; }

        internal FieldInfo FeedOptionsFieldInfo { get; }

        private ConcurrentDictionary<string, PropertyInfo[]> PropertyCache { get; } = new ConcurrentDictionary<string, PropertyInfo[]>();

        private ConcurrentDictionary<Tuple<string, string>, FieldInfo> FieldInfoCache { get; } = new ConcurrentDictionary<Tuple<string, string>, FieldInfo>();

        internal FieldInfo GetFieldInfoFromCache(Type type, string fieldName, BindingFlags bindingFlags)
        {
            var entityName = type.AssemblyQualifiedName;

            var key = new Tuple<string, string>(entityName, fieldName);
            if (FieldInfoCache.ContainsKey(key))
            {
                FieldInfoCache.TryGetValue(key, out var field);
                return field;
            }

            var fieldInfo = type.GetTypeInfo().GetField(fieldName, bindingFlags);

            if (IsAnonymousType(type))
                return fieldInfo;

            FieldInfoCache.TryAdd(key, fieldInfo);
            return fieldInfo;
        }

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

            var documentQueryProviderTypeInfo = Type.GetType($"Microsoft.Azure.Documents.Linq.DocumentQueryProvider{LibVersion}").GetTypeInfo();

            DocumentFeedOrDbLinkFieldInfo = documentQueryProviderTypeInfo.GetField("documentsFeedOrDatabaseLink", BindingFlags.Instance | BindingFlags.NonPublic);
            FeedOptionsFieldInfo = documentQueryProviderTypeInfo.GetField("feedOptions", BindingFlags.Instance | BindingFlags.NonPublic);
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