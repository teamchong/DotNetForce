﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DotNetForce.Common.Internals;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Serializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce.Common
{
    public class JsonHttpClient : BaseHttpClient, IJsonHttpClient
    {
        private const string DateFormat = "s";

        public JsonHttpClient(string instanceUrl, string apiVersion, string accessToken, HttpClient httpClient)
            : base(instanceUrl, apiVersion, "application/json", httpClient)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        // GET

        public async Task<T> HttpGetAsync<T>(string urlSuffix)
        {
            var url = Common.FormatUrl(urlSuffix, InstanceUrl, ApiVersion);
            return await HttpGetAsync<T>(url);
        }

        public async Task<T> HttpGetAsync<T>(Uri uri)
        {
            try
            {
                //var response = await HttpGetAsync(uri);
                //var jToken = JsonConvert.DeserializeObject<JToken>(response);
                //if (jToken.Type == JTokenType.Array)
                //{
                //    var jArray = JsonConvert.DeserializeObject<JToken>(response);
                //    return JsonConvert.DeserializeObject<T>(jArray.ToString());
                //}
                // else
                //try
                //{
                //    ////var jObject = JsonConvert.DeserializeObject<JToken>(response);
                //    ////return JsonConvert.DeserializeObject<T>(jObject.ToString());
                //}
                //catch
                //{
                //    return JsonConvert.DeserializeObject<T>(response);
                //}
                var jToken = await HttpGetJsonAsync(uri);
                return jToken.ToObject<T>();
            }
            catch (BaseHttpClientException e)
            {
                throw ParseForceException(e.Message);
            }
        }

        public async Task<IList<T>> HttpGetAsync<T>(string urlSuffix, string nodeName)
        {
            string next = null;
            var records = new List<T>();
            var url = Common.FormatUrl(urlSuffix, InstanceUrl, ApiVersion);

            do
            {
                if (next != null) url = Common.FormatUrl($"query/{next.Split('/').Last()}", InstanceUrl, ApiVersion);
                try
                {
                    //var response = await HttpGetAsync(url);
                    //var jObject = JsonConvert.DeserializeObject<JToken>(response);
                    //var jToken = jObject.GetValue(nodeName);
                    //next = (jObject.GetValue("nextRecordsUrl") != null) ? jObject.GetValue("nextRecordsUrl").ToString() : null;
                    //records.AddRange(JsonConvert.DeserializeObject<IList<T>>(jToken.ToString()));

                    var jObject = await HttpGetJsonAsync(url);
                    var jToken = jObject[nodeName];
                    next = jObject["nextRecordsUrl"]?.ToString();
                    records.AddRange(jToken?.ToObject<IList<T>>() ?? new List<T>());
                }
                catch (BaseHttpClientException e)
                {
                    throw ParseForceException(e.Message);
                }
            } while (!string.IsNullOrEmpty(next));

            return records;
        }

        public async Task<T> HttpGetRestApiAsync<T>(string apiName)
        {
            var url = Common.FormatRestApiUrl(apiName, InstanceUrl);
            return await HttpGetAsync<T>(url);
        }

        // POST

        public async Task<T> HttpPostAsync<T>(object inputObject, string urlSuffix)
        {
            var url = Common.FormatUrl(urlSuffix, InstanceUrl, ApiVersion);
            return await HttpPostAsync<T>(inputObject, url);
        }

        public async Task<T> HttpPostAsync<T>(object inputObject, Uri uri)
        {
            var json = JsonConvert.SerializeObject(inputObject,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CreatableContractResolver(),
                    DateFormatString = DateFormat
                });
            try
            {
                //var response = await HttpPostAsync(json, uri);
                //return JsonConvert.DeserializeObject<T>(response);
                var response = await HttpPostJsonAsync(json, uri);
                return response.ToObject<T>();
            }
            catch (BaseHttpClientException e)
            {
                throw ParseForceException(e.Message);
            }
        }

        public async Task<T> HttpPostRestApiAsync<T>(string apiName, object inputObject)
        {
            var url = Common.FormatRestApiUrl(apiName, InstanceUrl);
            return await HttpPostAsync<T>(inputObject, url);
        }

        public async Task<T> HttpBinaryDataPostAsync<T>(string urlSuffix, object inputObject, byte[] fileContents, string headerName, string fileName)
        {
            // BRAD: I think we should probably, in time, refactor multipart and binary support to the BaseHttpClient.
            // For now though, I just left this in here.

            var uri = Common.FormatUrl(urlSuffix, InstanceUrl, ApiVersion);

            var json = JsonConvert.SerializeObject(inputObject,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            var content = new MultipartFormDataContent();

            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            stringContent.Headers.Add("Content-Disposition", "form-data; name=\"json\"");
            content.Add(stringContent);

            var byteArrayContent = new ByteArrayContent(fileContents);
            byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
            byteArrayContent.Headers.Add("Content-Disposition", $"form-data; name=\"{headerName}\"; filename=\"{fileName}\"");
            content.Add(byteArrayContent, headerName, fileName);

            var responseMessage = await HttpClient.PostAsync(DnfClient.Proxy(uri), content).ConfigureAwait(false);
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseMessage.IsSuccessStatusCode) return JsonConvert.DeserializeObject<T>(response);

            throw ParseForceException(response);
        }

        // PATCH

        public async Task<SuccessResponse> HttpPatchAsync(object inputObject, string urlSuffix)
        {
            var url = Common.FormatUrl(urlSuffix, InstanceUrl, ApiVersion);
            return await HttpPatchAsync(inputObject, url);
        }

        public async Task<SuccessResponse> HttpPatchAsync(object inputObject, string urlSuffix, bool ignoreNull)
        {
            var url = Common.FormatUrl(urlSuffix, InstanceUrl, ApiVersion);
            if (ignoreNull) return await HttpPatchAsync(inputObject, url);
            return await HttpPatchAsync(inputObject, url, NullValueHandling.Include);
            //   return await HttpPatchAsync(inputObject, url, ignoreNull);
        }


        public async Task<SuccessResponse> HttpPatchAsync(object inputObject, Uri uri)
        {
            var json = JsonConvert.SerializeObject(inputObject,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new UpdateableContractResolver(),
                    DateFormatString = DateFormat
                });
            try
            {
                var response = await base.HttpPatchAsync(json, uri);
                return string.IsNullOrEmpty(response) ? new SuccessResponse { Id = "", Errors = "", Success = true } : JsonConvert.DeserializeObject<SuccessResponse>(response);
            }
            catch (BaseHttpClientException e)
            {
                throw ParseForceException(e.Message);
            }
        }

        public async Task<SuccessResponse> HttpPatchAsync(object inputObject, Uri uri, NullValueHandling nullValueHandling)
        {
            var json = JsonConvert.SerializeObject(inputObject,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = nullValueHandling,
                    ContractResolver = new UpdateableContractResolver(),
                    DateFormatString = DateFormat
                });


            try
            {
                var response = await base.HttpPatchAsync(json, uri);
                return string.IsNullOrEmpty(response) ? new SuccessResponse { Id = "", Errors = "", Success = true } : JsonConvert.DeserializeObject<SuccessResponse>(response);
            }
            catch (BaseHttpClientException e)
            {
                throw ParseForceException(e.Message);
            }
        }

        // DELETE

        public async Task<bool> HttpDeleteAsync(string urlSuffix)
        {
            var url = Common.FormatUrl(urlSuffix, InstanceUrl, ApiVersion);
            return await HttpDeleteAsync(url);
        }

        public new async Task<bool> HttpDeleteAsync(Uri uri)
        {
            try
            {
                await base.HttpDeleteAsync(uri);
                return true;
            }
            catch (BaseHttpClientException e)
            {
                throw ParseForceException(e.Message);
            }
        }

        private static ForceException ParseForceException(string responseMessage)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponses>(responseMessage);
                return new ForceException(errorResponse?[0]?.ErrorCode, errorResponse?[0].Message);
            }
            catch (JsonSerializationException)
            {
                return new ForceException(Error.NonJsonErrorResponse, responseMessage);
            }
        }


        public async Task<Stream> HttpGetBlobAsync(string urlSuffix)
        {
            var url = Common.FormatUrl(urlSuffix, InstanceUrl, ApiVersion);

            var request = new HttpRequestMessage
            {
                RequestUri = DnfClient.Proxy(url),
                Method = HttpMethod.Get
            };

            var responseMessage = await HttpClient.SendAsync(request).ConfigureAwait(false);
            var response = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (responseMessage.IsSuccessStatusCode) return response;
            return new MemoryStream();
        }

        // Get/Post/Patch/Delete
        private static JToken DeserializeJson(StreamReader streamReader)
        {
            using var jsonReader = new JsonTextReader(streamReader);
            return new JsonSerializer().Deserialize<JToken>(jsonReader);
        }

        private async Task<JToken> HttpGetJsonAsync(Uri uri)
        {
            var responseMessage = await HttpClient.GetAsync(DnfClient.Proxy(uri)).ConfigureAwait(false);
            ParseApiUsage(responseMessage);

            if (responseMessage.StatusCode == HttpStatusCode.NoContent) return string.Empty;

            var response = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            using var reader = new StreamReader(response);
            if (responseMessage.IsSuccessStatusCode) return DeserializeJson(reader);

            throw new BaseHttpClientException(await reader.ReadToEndAsync().ConfigureAwait(false), responseMessage.StatusCode);
        }

        private async Task<JToken> HttpPostJsonAsync(string payload, Uri uri)
        {
            //var content = new StringContent(payload, Encoding.UTF8, _contentType);
            var content = !DnfClient.UseCompression ? (HttpContent)new StringContent(payload, Encoding.UTF8, _contentType) : GetGZipContent(payload);

            var responseMessage = await HttpClient.PostAsync(DnfClient.Proxy(uri), content).ConfigureAwait(false);
            ParseApiUsage(responseMessage);


            if (responseMessage.StatusCode == HttpStatusCode.NoContent) return string.Empty;

            var response = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            using var reader = new StreamReader(response);
            if (responseMessage.IsSuccessStatusCode) return DeserializeJson(reader);

            throw new BaseHttpClientException(await reader.ReadToEndAsync().ConfigureAwait(false), responseMessage.StatusCode);
        }
    }
}
