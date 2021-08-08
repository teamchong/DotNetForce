using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DotNetForce.Common.Internals;
using DotNetForce.Common.Models.Json;

namespace DotNetForce.Common
{
    public class XmlHttpClient : BaseHttpClient, IXmlHttpClient
    {
        public XmlHttpClient(string? instanceUrl, string apiVersion, string? accessToken, HttpClient httpClient)
            : base(instanceUrl, apiVersion, "application/xml", httpClient)
        {
            if (ApiVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase)) ApiVersion = ApiVersion[1..];
            HttpClient.DefaultRequestHeaders.Add("X-SFDC-Session", accessToken);
        }

        // GET

        public Task<T?> HttpGetAsync<T>(string resourceName) where T : class
        {
            var uri = Common.FormatUrl(resourceName, InstanceUrl, ApiVersion);
            return HttpGetAsync<T>(uri);
        }

        public async Task<T?> HttpGetAsync<T>(Uri uri) where T : class
        {
            try
            {
                //var response = await HttpGetAsync(uri).ConfigureAwait(false);
                //return DeserializeXmlString<T>(response);
                return await HttpGetXmlAsync<T>(uri)
                    .ConfigureAwait(false);
            }
            catch (BaseHttpClientException e)
            {
                throw ParseForceException(e.Message);
            }
        }

        // POST

        public Task<T?> HttpPostAsync<T>(object? inputObject, string resourceName) where T : class
        {
            var uri = Common.FormatUrl(resourceName, InstanceUrl, ApiVersion);
            return HttpPostAsync<T>(inputObject, uri);
        }

        public async Task<T?> HttpPostAsync<T>(object? inputObject, Uri uri) where T : class
        {
            if (inputObject == null) throw new ArgumentNullException(nameof(inputObject));
            var payload = SerializeXmlObject(inputObject);
            try
            {
                //var response = await HttpPostAsync(postBody, uri).ConfigureAwait(false);
                //return DeserializeXmlString<T>(response);
                return await HttpPostXmlAsync<T>(payload, uri)
                    .ConfigureAwait(false);
            }
            catch (BaseHttpClientException e)
            {
                throw ParseForceException(e.Message);
            }
        }

        // HELPER METHODS

        private static ForceException ParseForceException(string responseMessage)
        {
            var errorResponse = DeserializeXmlString<ErrorResponse>(responseMessage);
            return new ForceException(errorResponse.ErrorCode ?? string.Empty, errorResponse.Message ?? string.Empty);
        }

        private static string SerializeXmlObject(object inputObject)
        {
            var xmlSerializer = new XmlSerializer(inputObject.GetType());
            var stringWriter = new StringWriter();
            using var writer = XmlWriter.Create(stringWriter);
            xmlSerializer.Serialize(writer, inputObject);
            var result = stringWriter.ToString();
            return result;
        }

        private static T DeserializeXmlString<T>(string inputString)
        {
            var serializer = new XmlSerializer(typeof(T));
            using TextReader reader = new StringReader(inputString);
            var result = (T)serializer.Deserialize(reader);
            return result;
        }

        // Get/Post/Patch/Delete
        private static T DeserializeXml<T>(TextReader streamReader)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new XmlTextReader(streamReader);
            var result = (T)serializer.Deserialize(reader);
            return result;
        }

        private async Task<T?> HttpGetXmlAsync<T>(Uri uri) where T : class
        {
            var responseMessage = await HttpClient.GetAsync(DnfClient.Proxy(uri))
                .ConfigureAwait(false);
            ParseApiUsage(responseMessage);

            var response = await responseMessage.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);

            using var reader = new StreamReader(response);
            if (responseMessage.IsSuccessStatusCode) return DeserializeXml<T>(reader);

            throw new BaseHttpClientException(await reader.ReadToEndAsync()
                .ConfigureAwait(false), responseMessage.StatusCode);
        }

        private async Task<T?> HttpPostXmlAsync<T>(string payload, Uri uri) where T : class
        {
            //var content = new StringContent(payload, Encoding.UTF8, _contentType);
            var content = !DnfClient.UseCompression ? (HttpContent)new StringContent(payload, Encoding.UTF8, _contentType) : GetGZipContent(payload);

            var responseMessage = await HttpClient.PostAsync(DnfClient.Proxy(uri), content)
                .ConfigureAwait(false);
            ParseApiUsage(responseMessage);

            var response = await responseMessage.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);

            using var reader = new StreamReader(response);
            if (responseMessage.IsSuccessStatusCode) return DeserializeXml<T>(reader);

            throw new BaseHttpClientException(await reader.ReadToEndAsync()
                .ConfigureAwait(false), responseMessage.StatusCode);
        }
    }
}
