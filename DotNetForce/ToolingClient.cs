using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Soql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DotNetForce
{
    internal class ToolingClient : IToolingClient
    {
        public ISelectListResolver SelectListResolver { get; set; }

        protected JsonHttpClient JsonHttp { get; }

        public ToolingClient(JsonHttpClient jsonHttp)
        {
            SelectListResolver = new DataMemberSelectListResolver();
            JsonHttp = jsonHttp;
        }


        public Task<DescribeGlobalResult<JObject>?> GetObjectsAsync() => GetObjectsAsync<JObject>();
        public Task<DescribeGlobalResult<T>?> GetObjectsAsync<T>()
        {
            const string? resourceName = "tooling/sobjects";
            return JsonHttp.HttpGetAsync<DescribeGlobalResult<T>>(resourceName);
        }

        public Task<JObject?> BasicInformationAsync(MetadataType metadataType) => BasicInformationAsync<JObject>(metadataType);

        public Task<T?> BasicInformationAsync<T>(MetadataType metadataType) where T : class
        {
            var resourceName = $"tooling/sobjects/{metadataType}";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> DescribeAsync(MetadataType metadataType) => DescribeAsync<JObject>(metadataType);
        public Task<T?> DescribeAsync<T>(MetadataType metadataType) where T : class
        {
            var resourceName = $"tooling/sobjects/{metadataType}/describe";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public IAsyncEnumerable<QueryResult<JObject>> QueryAsync(string q) => QueryAsync<JObject>(q);
        public async IAsyncEnumerable<QueryResult<T>> QueryAsync<T>(string q)
        {
            if (string.IsNullOrEmpty(q)) throw new ArgumentNullException(nameof(q));

            var resourceName = $"tooling/query?q={Dnf.EscapeDataString(q)}";
            var result = await JsonHttp.HttpGetAsync<QueryResult<T>>(resourceName)
                .ConfigureAwait(false);
            await foreach (var nextResult in QueryByLocatorAsync(result)
                .ConfigureAwait(false))
                yield return nextResult;
        }

        public IAsyncEnumerable<QueryResult<JObject>> SearchAsync(string q) => SearchAsync<JObject>(q);
        public async IAsyncEnumerable<QueryResult<T>> SearchAsync<T>(string q)
        {
            if (string.IsNullOrEmpty(q)) throw new ArgumentNullException(nameof(q));

            var resourceName = $"tooling/search?q={Dnf.EscapeDataString(q)}";
            var result = await JsonHttp.HttpGetAsync<QueryResult<T>>(resourceName)
                .ConfigureAwait(false);
            await foreach (var nextResult in QueryByLocatorAsync(result)
                .ConfigureAwait(false))
                yield return nextResult;
        }


        public async IAsyncEnumerable<QueryResult<T>> QueryByLocatorAsync<T>(QueryResult<T>? queryResult)
        {
            if (queryResult == null) yield break;
            yield return queryResult;
            while (!string.IsNullOrEmpty(queryResult.NextRecordsUrl))
            {
                var resourceName = queryResult.NextRecordsUrl;
                queryResult = await JsonHttp.HttpGetAsync<QueryResult<T>>(resourceName)
                    .ConfigureAwait(false);
                if (queryResult == null) yield break;
                yield return queryResult;
            }
        }

        public Task<JObject?> QueryByIdAsync(string objectName, string recordId)
        {
            return QueryByIdAsync<JObject>(objectName, recordId);
        }

        public async Task<T?> QueryByIdAsync<T>(string objectName, string recordId) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));

            var fields = SelectListResolver.GetFieldsList<T>();
            var query = $"SELECT {fields} FROM {objectName} WHERE Id='{recordId}'";
            await foreach (var result in QueryAsync<T>(query)
                .ConfigureAwait(false))
                return result.Records?.FirstOrDefault();
            return default;
        }

        public async Task<SaveResponse> CreateAsync(MetadataType metadataType, object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            var resourceName = $"tooling/sobjects/{metadataType}";
            return await JsonHttp.HttpPostAsync<SaveResponse>(record, resourceName)
                .ConfigureAwait(false) ?? new SaveResponse();
        }
        
        public Task<JObject?> RetrieveAsync(MetadataType metadataType, string recordId, params string[] fields) => RetrieveAsync<JObject>(metadataType, recordId, fields);
        public Task<T?> RetrieveAsync<T>(MetadataType metadataType, string recordId, params string[] fields) where T : class
        {
            var resourceName = fields.Length > 0
                ? $"tooling/sobjects/{metadataType}/{recordId}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                : $"tooling/sobjects/{metadataType}/{recordId}";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public Task<SuccessResponse> UpdateAsync(MetadataType metadataType, object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            var body = JObject.FromObject(record);
            return UpdateAsync(metadataType, body["Id"]?.ToString() ?? string.Empty, Dnf.Omit(body, "Id"));
        }

        public Task<SuccessResponse> UpdateAsync(MetadataType metadataType, string recordId, object record)
        {
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var resourceName = $"tooling/sobjects/{metadataType}/{recordId}";
            return JsonHttp.HttpPatchAsync(record, resourceName);
        }

        public Task<bool> DeleteAsync(MetadataType metadataType, string recordId)
        {
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));

            var resourceName = $"tooling/sobjects/{metadataType}/{recordId}";
            return JsonHttp.HttpDeleteAsync(resourceName);
        }


        public async Task<JToken> CompletionsAsync(string type)
        {
            if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));

            var resourceName = $"tooling/completions?type={Uri.EscapeDataString(type)}";
            return await JsonHttp.HttpGetAsync<JToken>(resourceName)
                .ConfigureAwait(false) ?? JValue.CreateNull();
        }

        public async Task<ExecuteAnonymousResult> ExecuteAnonymousAsync(string anonymousBody)
        {
            if (string.IsNullOrEmpty(anonymousBody)) throw new ArgumentNullException(nameof(anonymousBody));

            var resourceName = $"tooling/executeAnonymous?anonymousBody={Dnf.EscapeDataString(anonymousBody)}";
            return await JsonHttp.HttpGetAsync<ExecuteAnonymousResult>(resourceName)
                .ConfigureAwait(false) ?? new ExecuteAnonymousResult();
        }

        public async Task<JToken> RunTestsAsynchronousAsync(JToken inputObject)
        {
            if (inputObject == null) throw new ArgumentNullException(nameof(inputObject));

            const string? resourceName = "tooling/runTestsAsynchronous";
            return await JsonHttp.HttpPostAsync<JToken>(inputObject, resourceName)
                .ConfigureAwait(false) ?? JValue.CreateNull();
        }

        public async Task<JToken> RunTestsSynchronousAsync(JToken inputObject)
        {
            if (inputObject == null) throw new ArgumentNullException(nameof(inputObject));

            const string resourceName = "tooling/runTestsSynchronous";
            return await JsonHttp.HttpPostAsync<JToken>(inputObject, resourceName)
                .ConfigureAwait(false) ?? JValue.CreateNull();
        }
    }
}
