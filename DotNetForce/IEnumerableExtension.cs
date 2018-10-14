using DotNetForce;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Models.Xml;
using DotNetForce.Force;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DotNetForce
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<List<TSource>> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
            var bucket = new List<TSource>();
            //var count = 0;

            foreach (var item in source)
            {
                bucket.Add(item);
                if (bucket.Count < size)
                    continue;

                yield return bucket;

                bucket = new List<TSource>();
                //count = 0;
            }

            if (bucket.Count > 0)
            {
                yield return bucket;
            }
        }
        
        public static Task<CompositeResult> CreateAsync<TSource>(this IEnumerable<TSource> source, DNFClient client)
        {
            return client.Composite.CreateAsync(source);
        }

        public static Task<CompositeResult> CreateAsync<TSource>(this IEnumerable<TSource> source, DNFClient client, bool allOrNone)
        {
            return client.Composite.CreateAsync(source, allOrNone);
        }

        public static Task<SaveResponse> CreateTreeAsync<TSource>(this IEnumerable<TSource> source, DNFClient client, string objectName)
            where TSource : IAttributedObject
        {
            return client.Composite.CreateTreeAsync(objectName, source);
        }

        public static Task<CompositeResult> RetrieveAsync(this IEnumerable<string> source, DNFClient client, string objectName, params string[] fields)
        {
            return client.Composite.RetrieveAsync(objectName, source, fields);
        }

        public static Task<CompositeResult> RetrieveExternalAsync(this IEnumerable<string> source, DNFClient client, string objectName, string externalFieldName, params string[] fields)
        {
            return client.Composite.RetrieveExternalAsync(objectName, externalFieldName, source, fields);
        }

        public static Task<CompositeResult> UpdateAsync<TSource>(this IEnumerable<TSource> source, DNFClient client)
        {
            return client.Composite.UpdateAsync(source);
        }

        public static Task<CompositeResult> UpdateAsync<TSource>(this IEnumerable<TSource> source, DNFClient client, bool allOrNone)
        {
            return client.Composite.UpdateAsync(source, allOrNone);
        }

        //public static Task<CompositeResult> UpsertExternalAsync<TSource>(this IEnumerable<TSource> source, DNFClient client, string externalFieldName)
        //{
        //    return client.Composite.UpsertExternalAsync(externalFieldName, source);
        //}

        //public static Task<CompositeResult> UpsertExternalAsync<TSource>(this IEnumerable<TSource> source, DNFClient client, string externalFieldName, bool allOrNone)
        //{
        //    return client.Composite.UpsertExternalAsync(externalFieldName, source, allOrNone);
        //}

        public static Task<CompositeResult> DeleteAsync(this IEnumerable<string> source, DNFClient client)
        {
            return client.Composite.DeleteAsync(source);
        }

        public static Task<CompositeResult> DeleteAsync(this IEnumerable<string> source, DNFClient client, bool allOrNone)
        {
            return client.Composite.DeleteAsync(source, allOrNone);
        }
    }
}
