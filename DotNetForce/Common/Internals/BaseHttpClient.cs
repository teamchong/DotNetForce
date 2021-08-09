using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace DotNetForce.Common.Internals
{
    public abstract class BaseHttpClient : IDisposable
    {
        protected const string UserAgent = "dotnetforce";

        // ReSharper disable once InconsistentNaming
        protected readonly string _contentType;
        protected readonly HttpClient HttpClient;

        protected readonly string? InstanceUrl;

        public DateTime? ApiLastRetrieve;
        public int? ApiLimit;
        public int? ApiUsed;
        protected string ApiVersion;
        public DateTime? PerAppApiLastRetrieve;
        public int? PerAppApiLimit;
        public int? PerAppApiUsed;

        internal BaseHttpClient(string? instanceUrl, string apiVersion, string contentType, HttpClient httpClient)
        {
            if (string.IsNullOrEmpty(instanceUrl)) throw new ArgumentNullException(nameof(instanceUrl));
            if (string.IsNullOrEmpty(apiVersion)) throw new ArgumentNullException(nameof(apiVersion));
            if (string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));

            InstanceUrl = instanceUrl;
            ApiVersion = apiVersion;
            _contentType = contentType;
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            HttpClient.DefaultRequestHeaders.UserAgent.Clear();
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(string.Concat(UserAgent, "/", ApiVersion));

            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_contentType));

            //HttpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
            //HttpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public string GetApiVersion()
        {
            return ApiVersion;
        }

        public void ParseApiUsage(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.Headers.Contains("Sforce-Limit-Info")) return;
            foreach (var limitInfo in responseMessage.Headers.GetValues("Sforce-Limit-Info"))
            {
                var split = limitInfo.Split(',');
                foreach (var str in split)
                    if (str.Trim().StartsWith("api-usage="))
                    {
                        var usage = str.Trim()["api-usage=".Length..].Split('/');
                        if (usage.Length != 2 || !int.TryParse(usage[0], out var apiUsed) || !int.TryParse(usage[1], out var apiLimit)) continue;
                        ApiLastRetrieve = DateTime.Now;
                        ApiUsed = apiUsed;
                        ApiLimit = apiLimit;
                    }
                    else if (str.Trim().StartsWith("per-app-api-usage="))
                    {
                        var usage = str.Trim()["per-app-api-usage=".Length..].Split('/');
                        if (usage.Length != 2 || !int.TryParse(usage[0], out var apiUsed) || !int.TryParse(usage[1], out var apiLimit)) continue;
                        PerAppApiLastRetrieve = DateTime.Now;
                        PerAppApiUsed = apiUsed;
                        PerAppApiLimit = apiLimit;
                    }
            }
        }

        protected StreamContent GetGZipContent(string payload)
        {
            var ms = new MemoryStream();
            var payloadData = Encoding.UTF8.GetBytes(payload);
            using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                gzip.Write(payloadData, 0, payloadData.Length);
            }
            ms.Position = 0;
            var streamContent = new StreamContent(ms);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(_contentType);
            streamContent.Headers.ContentEncoding.Add("gzip");
            return streamContent;
        }

        protected async Task<string> HttpGetAsync(Uri uri)
        {
            var responseMessage = await HttpClient.GetAsync(DnfClient.Proxy(uri))
                .ConfigureAwait(false);

            if (responseMessage.StatusCode == HttpStatusCode.NoContent) return string.Empty;

            var response = await responseMessage.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode) return response;

            throw new BaseHttpClientException(response, responseMessage.StatusCode);
        }

        protected async Task<string> HttpPostAsync(string payload, Uri uri)
        {
            //var content = new StringContent(payload, Encoding.UTF8, _contentType);
            var content = !DnfClient.UseCompression ? (HttpContent)new StringContent(payload, Encoding.UTF8, _contentType) : GetGZipContent(payload);

            var responseMessage = await HttpClient.PostAsync(DnfClient.Proxy(uri), content)
                .ConfigureAwait(false);
            ParseApiUsage(responseMessage);


            if (responseMessage.StatusCode == HttpStatusCode.NoContent) return string.Empty;

            var response = await responseMessage.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode) return response;

            throw new BaseHttpClientException(response, responseMessage.StatusCode);
        }

        protected async Task<string> HttpPatchAsync(string payload, Uri uri)
        {
            //var content = new StringContent(payload, Encoding.UTF8, _contentType);
            var content = !DnfClient.UseCompression ? (HttpContent)new StringContent(payload, Encoding.UTF8, _contentType) : GetGZipContent(payload);

            var request = new HttpRequestMessage
            {
                RequestUri = DnfClient.Proxy(uri),
                Method = new HttpMethod("PATCH"),
                Content = content
            };

            var responseMessage = await HttpClient.SendAsync(request)
                .ConfigureAwait(false);
            ParseApiUsage(responseMessage);

            if (responseMessage.StatusCode == HttpStatusCode.NoContent) return string.Empty;

            var response = await responseMessage.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode) return response;

            throw new BaseHttpClientException(response, responseMessage.StatusCode);
        }

        protected async Task<string> HttpDeleteAsync(Uri uri)
        {
            var responseMessage = await HttpClient.DeleteAsync(DnfClient.Proxy(uri))
                .ConfigureAwait(false);
            ParseApiUsage(responseMessage);

            if (responseMessage.StatusCode == HttpStatusCode.NoContent) return string.Empty;

            var response = await responseMessage.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode) return response;

            throw new BaseHttpClientException(response, responseMessage.StatusCode);
        }
    }
}
