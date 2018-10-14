using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Force;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DotNetForce
{
    public class ToolingClient : IToolingClient
    {
        protected JsonHttpClient JsonHttp { get; set; }

        public ToolingClient(JsonHttpClient jsonHttp)
        {
            JsonHttp = jsonHttp;
        }


        public async Task<DescribeGlobalResult<T>> GetObjectsAsync<T>()
        {
            var urlSuffix = "tooling/sobjects";
            return await JsonHttp.HttpGetAsync<DescribeGlobalResult<T>>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<T> BasicInformationAsync<T>(MetadataType metadataType)
        {
            var urlSuffix = $"tooling/sobjects/{metadataType}";
            return await JsonHttp.HttpGetAsync<T>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<T> DescribeAsync<T>(MetadataType metadataType)
        {
            var urlSuffix = $"tooling/sobjects/{metadataType}/describe";
            return await JsonHttp.HttpGetAsync<T>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<QueryResult<T>> QueryAsync<T>(string q)
        {
            if (string.IsNullOrEmpty(q)) throw new ArgumentNullException("q");

            var urlSuffix = $"tooling/query?q={Uri.EscapeDataString(q)}";
            return await JsonHttp.HttpGetAsync<QueryResult<T>>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<QueryResult<T>> SearchAsync<T>(string q)
        {
            if (string.IsNullOrEmpty(q)) throw new ArgumentNullException("q");

            var urlSuffix = $"tooling/search?q={Uri.EscapeDataString(q)}";
            return await JsonHttp.HttpGetAsync<QueryResult<T>>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<SaveResponse> CreateAsync(MetadataType metadataType, object record)
        {
            if (record == null) throw new ArgumentNullException("record");

            var urlSuffix = $"tooling/sobjects/{metadataType}";
            return await JsonHttp.HttpPostAsync<SaveResponse>(record, urlSuffix).ConfigureAwait(false);
        }

        public Task<T> RetreiveAsync<T>(MetadataType metadataType, string recordId)
        {
            return RetreiveAsync<T>(metadataType, recordId, null);
        }

        public async Task<T> RetreiveAsync<T>(MetadataType metadataType, string recordId, string[] fields)
        {
            var urlSuffix = fields?.Length > 0
                ? $"tooling/sobjects/{metadataType}/{recordId}?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}"
                : $"tooling/sobjects/{metadataType}/{recordId}";
            return await JsonHttp.HttpGetAsync<T>(urlSuffix).ConfigureAwait(false);
        }

        public Task<SuccessResponse> UpdateAsync(MetadataType metadataType, object record)
        {
            if (record == null) throw new ArgumentNullException("record");

            var body = JObject.FromObject(record);
            return UpdateAsync(metadataType, body["Id"]?.ToObject<string>(), body.Omit("Id"));
        }

        public async Task<SuccessResponse> UpdateAsync(MetadataType metadataType, string recordId, object record)
        {
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (record == null) throw new ArgumentNullException("record");

            var urlSuffix = $"tooling/sobjects/{metadataType}/{recordId}";
            return await JsonHttp.HttpPatchAsync(record, urlSuffix).ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(MetadataType metadataType, string recordId)
        {
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");

            var urlSuffix = $"tooling/sobjects/{metadataType}/{recordId}";
            return await JsonHttp.HttpDeleteAsync(urlSuffix).ConfigureAwait(false);
        }


        public async Task<JToken> CompletionsAsync(string type)
        {
            if (string.IsNullOrEmpty(type)) throw new ArgumentNullException("type");

            var urlSuffix = $"tooling/completions?type={Uri.EscapeDataString(type)}";
            return await JsonHttp.HttpGetAsync<JToken>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<ExecuteAnonymousResult> ExecuteAnonymousAsync(string anonymousBody)
        {
            if (string.IsNullOrEmpty(anonymousBody)) throw new ArgumentNullException("anonymousBody");

            var urlSuffix = $"tooling/executeAnonymous?anonymousBody={Uri.EscapeDataString(anonymousBody)}";
            return await JsonHttp.HttpGetAsync<ExecuteAnonymousResult>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<JToken> RunTestsAsynchronousAsync(JObject inputObject)
        {
            if (inputObject == null) throw new ArgumentNullException("inputObject");

            var urlSuffix = "tooling/runTestsAsynchronous";
            return await JsonHttp.HttpPostAsync<JToken>(inputObject, urlSuffix).ConfigureAwait(false);
        }

        public async Task<JToken> RunTestsSynchronousAsync(JObject inputObject)
        {
            if (inputObject == null) throw new ArgumentNullException("inputObject");

            var urlSuffix = "tooling/runTestsSynchronous";
            return await JsonHttp.HttpPostAsync<JToken>(inputObject, urlSuffix).ConfigureAwait(false);
        }
    }
}
