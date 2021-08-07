using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    internal class ToolingClient : IToolingClient
    {
        public ToolingClient(JsonHttpClient jsonHttp)
        {
            JsonHttp = jsonHttp;
        }

        protected JsonHttpClient JsonHttp { get; set; }


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
            if (string.IsNullOrEmpty(q)) throw new ArgumentNullException(nameof(q));

            var urlSuffix = $"tooling/query?q={Dnf.EscapeDataString(q)}";
            return await JsonHttp.HttpGetAsync<QueryResult<T>>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<QueryResult<T>> SearchAsync<T>(string q)
        {
            if (string.IsNullOrEmpty(q)) throw new ArgumentNullException(nameof(q));

            var urlSuffix = $"tooling/search?q={Dnf.EscapeDataString(q)}";
            return await JsonHttp.HttpGetAsync<QueryResult<T>>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<SaveResponse> CreateAsync(MetadataType metadataType, object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            var urlSuffix = $"tooling/sobjects/{metadataType}";
            return await JsonHttp.HttpPostAsync<SaveResponse>(record, urlSuffix).ConfigureAwait(false);
        }

        public Task<T> RetrieveAsync<T>(MetadataType metadataType, string recordId)
        {
            return RetrieveAsync<T>(metadataType, recordId, null);
        }

        public async Task<T> RetrieveAsync<T>(MetadataType metadataType, string recordId, string[] fields)
        {
            var urlSuffix = fields?.Length > 0
                ? $"tooling/sobjects/{metadataType}/{recordId}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                : $"tooling/sobjects/{metadataType}/{recordId}";
            return await JsonHttp.HttpGetAsync<T>(urlSuffix).ConfigureAwait(false);
        }

        public Task<SuccessResponse> UpdateAsync(MetadataType metadataType, object record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            var body = JObject.FromObject(record);
            return UpdateAsync(metadataType, body["Id"]?.ToString(), Dnf.Omit(body, "Id"));
        }

        public async Task<SuccessResponse> UpdateAsync(MetadataType metadataType, string recordId, object record)
        {
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var urlSuffix = $"tooling/sobjects/{metadataType}/{recordId}";
            return await JsonHttp.HttpPatchAsync(record, urlSuffix).ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(MetadataType metadataType, string recordId)
        {
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));

            var urlSuffix = $"tooling/sobjects/{metadataType}/{recordId}";
            return await JsonHttp.HttpDeleteAsync(urlSuffix).ConfigureAwait(false);
        }


        public async Task<JToken> CompletionsAsync(string type)
        {
            if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));

            var urlSuffix = $"tooling/completions?type={Uri.EscapeDataString(type)}";
            return await JsonHttp.HttpGetAsync<JToken>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<ExecuteAnonymousResult> ExecuteAnonymousAsync(string anonymousBody)
        {
            if (string.IsNullOrEmpty(anonymousBody)) throw new ArgumentNullException(nameof(anonymousBody));

            var urlSuffix = $"tooling/executeAnonymous?anonymousBody={Dnf.EscapeDataString(anonymousBody)}";
            return await JsonHttp.HttpGetAsync<ExecuteAnonymousResult>(urlSuffix).ConfigureAwait(false);
        }

        public async Task<JToken> RunTestsAsynchronousAsync(JToken inputObject)
        {
            if (inputObject == null) throw new ArgumentNullException(nameof(inputObject));

            var urlSuffix = "tooling/runTestsAsynchronous";
            return await JsonHttp.HttpPostAsync<JToken>(inputObject, urlSuffix).ConfigureAwait(false);
        }

        public async Task<JToken> RunTestsSynchronousAsync(JToken inputObject)
        {
            if (inputObject == null) throw new ArgumentNullException(nameof(inputObject));

            var urlSuffix = "tooling/runTestsSynchronous";
            return await JsonHttp.HttpPostAsync<JToken>(inputObject, urlSuffix).ConfigureAwait(false);
        }
    }
}
