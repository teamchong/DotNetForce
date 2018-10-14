using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DotNetForce.Common.Internals
{
    public abstract class BaseHttpClient : IDisposable
    {
        private const string UserAgent = "forcedotcom-toolkit-dotnet";
        private readonly string _contentType;

        protected readonly string InstanceUrl;
        protected string ApiVersion;
        protected readonly HttpClient HttpClient;
        
        public int? ApiUsed = null;
        public int? ApiLimit = null;
        public int? PerAppApiUsed = null;
        public int? PerAppApiLimit = null;

        internal BaseHttpClient(string instanceUrl, string apiVersion, string contentType, HttpClient httpClient)
        {
            if (string.IsNullOrEmpty(instanceUrl)) throw new ArgumentNullException("instanceUrl");
            if (string.IsNullOrEmpty(apiVersion)) throw new ArgumentNullException("apiVersion");
            if (string.IsNullOrEmpty(contentType)) throw new ArgumentNullException("contentType");
            if (httpClient == null) throw new ArgumentNullException("httpClient");

            InstanceUrl = instanceUrl;
            ApiVersion = apiVersion;
            _contentType = contentType;
            HttpClient = httpClient;

            HttpClient.DefaultRequestHeaders.UserAgent.Clear();
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(string.Concat(UserAgent, "/", ApiVersion));

            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_contentType));

            //HttpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
            //HttpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }

        public string GetApiVersion()
        {
            return ApiVersion;
        }

        public void ParseApiUsage(HttpResponseMessage responseMessage)
        {
            if (responseMessage?.Headers?.Contains("Sforce-Limit-Info") == true)
            {
                foreach (var limitInfo in responseMessage.Headers.GetValues("Sforce-Limit-Info"))
                {
                    var splitted = limitInfo.Split(',');
                    foreach (var str in splitted)
                    {
                        if (str.Trim().StartsWith("api-usage="))
                        {
                            var usage = str.Trim().Substring("api-usage=".Length).Split('/');
                            if (usage.Length == 2 && int.TryParse(usage[0], out int apiUsed) && int.TryParse(usage[1], out int apiLimit))
                            {
                                ApiUsed = apiUsed;
                                ApiLimit = apiLimit;
                            }
                        }
                        else if (str.Trim().StartsWith("per-app-api-usage="))
                        {
                            var usage = str.Trim().Substring("per-app-api-usage=".Length).Split('/');
                            if (usage.Length == 2 && int.TryParse(usage[0], out int apiUsed) && int.TryParse(usage[1], out int apiLimit))
                            {
                                PerAppApiUsed = apiUsed;
                                PerAppApiLimit = apiLimit;
                            }
                        }
                    }
                }
            }
        }

        protected async Task<string> HttpGetAsync(Uri uri)
        {
            var responseMessage = await HttpClient.GetAsync(uri).ConfigureAwait(false);
            ParseApiUsage(responseMessage);

            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            throw new BaseHttpClientException(response, responseMessage.StatusCode);
        }

        protected async Task<string> HttpPostAsync(string payload, Uri uri)
        {
            var content = new StringContent(payload, Encoding.UTF8, _contentType);

            var responseMessage = await HttpClient.PostAsync(uri, content).ConfigureAwait(false);
            ParseApiUsage(responseMessage);


            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            throw new BaseHttpClientException(response, responseMessage.StatusCode);
        }

        protected async Task<string> HttpPatchAsync(string payload, Uri uri)
        {
            var content = new StringContent(payload, Encoding.UTF8, _contentType);

            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = new HttpMethod("PATCH"),
                Content = content
            };

            var responseMessage = await HttpClient.SendAsync(request).ConfigureAwait(false);
            ParseApiUsage(responseMessage);

            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            throw new BaseHttpClientException(response, responseMessage.StatusCode);
        }

        protected async Task<string> HttpDeleteAsync(Uri uri)
        {
            var responseMessage = await HttpClient.DeleteAsync(uri).ConfigureAwait(false);
            ParseApiUsage(responseMessage);

            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            throw new BaseHttpClientException(response, responseMessage.StatusCode);
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
