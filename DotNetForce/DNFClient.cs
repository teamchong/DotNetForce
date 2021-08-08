using DotNetForce.Chatter;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Soql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace DotNetForce
{
    // ReSharper disable once InconsistentNaming
    public class DnfClient : IDnfClient
    {   
        public ISelectListResolver SelectListResolver { get; set; }

        // ReSharper disable once ConvertToConstant.Global
        public static bool UseCompression = true;
        // ReSharper disable once ConvertToConstant.Global
        public static string DefaultApiVersion = "v52.0";
        public static Func<Uri, Uri> Proxy = uri => uri;

        #region Client

        public JsonHttpClient JsonHttp { get; set; }

        public XmlHttpClient XmlHttp { get; set; }

        public IChatterClient Chatter { get; set; }

        public ICompositeClient Composite { get; set; }

        public IToolingClient Tooling { get; set; }

        public IBulkClient Bulk { get; set; }

        public ILayoutClient Layout { get; set; }

        #endregion

        public DnfClient(string? instanceUrl, string? accessToken)
            : this(instanceUrl, accessToken, null, null) { }

        public DnfClient(string? instanceUrl, string? accessToken, Action<string> logger)
            : this(instanceUrl, accessToken, null, logger) { }

        public DnfClient(string? instanceUrl, string? accessToken, string? refreshToken, Action<string>? logger = null)
        {
            Logger = logger;
            SelectListResolver = new DataMemberSelectListResolver();
            HttpClient jsonClient;
            HttpClient xmlClient;

            if (UseCompression)
            {
                var httpHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                jsonClient = new HttpClient(httpHandler) { Timeout = TimeSpan.FromSeconds(60 * 30) };
                //jsonClient.DefaultRequestHeaders.ConnectionClose = true;
                jsonClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                xmlClient = new HttpClient(httpHandler) { Timeout = TimeSpan.FromSeconds(60 * 30) };
            }
            else
            {
                jsonClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60 * 30) };
                xmlClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60 * 30) };
            }

            //xmlClient.DefaultRequestHeaders.ConnectionClose = true;
            xmlClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            InstanceUrl = instanceUrl;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ApiVersion = DefaultApiVersion;

            JsonHttp = new JsonHttpClient(InstanceUrl, ApiVersion, AccessToken, jsonClient);
            XmlHttp = new XmlHttpClient(InstanceUrl, ApiVersion, AccessToken, xmlClient);
            
            Chatter = new ChatterClient(JsonHttp);
            Composite = new CompositeClient(JsonHttp, ApiVersion, Logger);
            Tooling = new ToolingClient(JsonHttp);
            Bulk = new BulkClient(XmlHttp, JsonHttp);
            Layout = new LayoutClient(XmlHttp, JsonHttp);
        }
        

        public string? InstanceUrl { get; set; }
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }

        public string ApiVersion { get; set; }

        public string? Id { get; set; }
        public string? IssuedAt { get; set; }
        public string? Signature { get; set; }

        public Action<string>? Logger { get; set; }

        public int? ApiUsed => JsonHttp.ApiLastRetrieve < XmlHttp.ApiLastRetrieve ? XmlHttp.ApiUsed : JsonHttp.ApiUsed;
        public int? ApiLimit => JsonHttp.ApiLastRetrieve < XmlHttp.ApiLastRetrieve ? XmlHttp.ApiLimit : JsonHttp.ApiLimit;
        public int? PerAppApiUsed => JsonHttp.PerAppApiLastRetrieve < XmlHttp.PerAppApiLastRetrieve ? XmlHttp.PerAppApiUsed : JsonHttp.PerAppApiUsed;
        public int? PerAppApiLimit => JsonHttp.PerAppApiLastRetrieve < XmlHttp.PerAppApiLastRetrieve ? XmlHttp.PerAppApiLimit : JsonHttp.PerAppApiLimit;

        public void Dispose()
        {
            JsonHttp.Dispose();
            XmlHttp.Dispose();
        }

        public Task<JObject?> LimitsAsync()
        {
            return LimitsAsync<JObject>();
        }

        public Task<T?> LimitsAsync<T>() where T : class
        {
            const string? resourceName = "limits";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public async Task<int> DailyApiUsed()
        {
            var limits = await LimitsAsync<JObject>()
                .ConfigureAwait(false);
            return (int?)limits?["DailyApiRequests"]?["Remaining"] ?? 0;
        }

        public async Task<int> DailyApiLimit()
        {
            var limits = await LimitsAsync<JObject>()
                .ConfigureAwait(false);
            return (int?)limits?["DailyApiRequests"]?["Max"] ?? 0;
        }

        public Task<IList<JObject>> VersionsAsync()
        {
            return VersionsAsync<JObject>();
        }

        public async Task<IList<T>> VersionsAsync<T>()
        {
            if (InstanceUrl == null) throw new ArgumentNullException(nameof(InstanceUrl));
            var uri = new Uri(new Uri(InstanceUrl), "/services/data");
            return await JsonHttp.HttpGetAsync<IList<T>>(uri)
                .ConfigureAwait(false) ?? new List<T>();
        }

        public Task<JObject?> ResourcesAsync()
        {
            return ResourcesAsync<JObject>(ApiVersion);
        }

        public Task<T?> ResourcesAsync<T>() where T : class
        {
            return ResourcesAsync<T>(ApiVersion);
        }

        public Task<JObject?> ResourcesAsync(string apiVersion)
        {
            return ResourcesAsync<JObject>(apiVersion);
        }

        public Task<T?> ResourcesAsync<T>(string apiVersion) where T : class
        {
            if (string.IsNullOrEmpty(apiVersion)) throw new ArgumentNullException(nameof(apiVersion));

            if (InstanceUrl == null) throw new ArgumentNullException(nameof(InstanceUrl));
            var uri = new Uri(new Uri(InstanceUrl), $"/services/data/{apiVersion}");
            return JsonHttp.HttpGetAsync<T>(uri);
        }

        #region Login

        public static async Task<DnfClient> LoginAsync(Uri loginUri, string clientId, string clientSecret, string userName, string password, Action<string>? logger = null)
        {
            if (loginUri == null) throw new ArgumentNullException(nameof(loginUri));
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));
            if (clientSecret == null) throw new ArgumentNullException(nameof(clientSecret));
            if (userName == null) throw new ArgumentNullException(nameof(userName));
            if (password == null) throw new ArgumentNullException(nameof(password));
            logger?.Invoke("DnfClient connecting...");
            var timer = Stopwatch.StartNew();

            using var auth = new AuthenticationClient { ApiVersion = DefaultApiVersion };
            var tokenRequestEndpointUrl = new Uri(new Uri(loginUri.GetLeftPart(UriPartial.Authority)), "/services/oauth2/token").ToString();
            await auth.UsernamePasswordAsync(clientId, clientSecret, userName, password, tokenRequestEndpointUrl)
                .ConfigureAwait(false);

            logger?.Invoke($"DnfClient connected ({timer.Elapsed.TotalSeconds} seconds)");

            var client = new DnfClient(auth.InstanceUrl, auth.AccessToken, auth.RefreshToken, logger) { Id = auth.Id };
            return client;
        }

        public static async Task<DnfClient> OAuthLoginAsync(OAuthProfile oAuthProfile, Action<string>? logger = null)
        {
            if (oAuthProfile.LoginUri == null) throw new ArgumentNullException(nameof(oAuthProfile.LoginUri));
            logger?.Invoke("DnfClient connecting...");
            var timer = Stopwatch.StartNew();

            using var auth = new AuthenticationClient { ApiVersion = DefaultApiVersion };
            var tokenRequestEndpointUrl = new Uri(new Uri(oAuthProfile.LoginUri.GetLeftPart(UriPartial.Authority)), "/services/oauth2/token").ToString();
            await auth.WebServerAsync(oAuthProfile.ClientId, oAuthProfile.ClientSecret, oAuthProfile.RedirectUri, oAuthProfile.Code, tokenRequestEndpointUrl)
                .ConfigureAwait(false);

            logger?.Invoke($"DnfClient connected ({timer.Elapsed.TotalSeconds} seconds)");

            var client = new DnfClient(auth.InstanceUrl, auth.AccessToken, auth.RefreshToken, logger) { Id = auth.Id };
            return client;
        }

        public async Task TokenRefreshAsync(Uri loginUri, string clientId, string clientSecret = "")
        {
            if (loginUri == null) throw new ArgumentNullException(nameof(loginUri));
            using var auth = new AuthenticationClient { ApiVersion = ApiVersion };
            var tokenRequestEndpointUrl = new Uri(new Uri(loginUri.GetLeftPart(UriPartial.Authority)), "/services/oauth2/token").ToString();
            await auth.TokenRefreshAsync(clientId, RefreshToken, clientSecret, tokenRequestEndpointUrl)
                .ConfigureAwait(false);
            Id = auth.Id;

            //Id = auth.Id;
            InstanceUrl = auth.InstanceUrl;
            AccessToken = auth.AccessToken;
        }

        #endregion


        #region STANDARD

        public Task<JObject?> ExplainAsync(string query)
        {
            return ExplainAsync<JObject>(query);
        }

        public async Task<T?> ExplainAsync<T>(string query) where T: class
        {
            var request = new CompositeRequest();
            request.Explain("q", query);
            var result = await Composite.PostAsync(request)
                .ConfigureAwait(false);
            result.Assert();
            return result.Results("q").ToObject<T>();
        }

        public IAsyncEnumerable<QueryResult<JObject>> QueryAsync(string query)
        {
            return QueryAsync<JObject>(query);
        }

        public async IAsyncEnumerable<QueryResult<T>> QueryAsync<T>(string query)
        {
            var request = new CompositeRequest();
            request.Query("q", query);
            var compositeResult = await Composite.PostAsync(request)
                .ConfigureAwait(false);
            compositeResult.Assert();
            var result = compositeResult.Queries<T>("q");
            await foreach (var nextResult in QueryByLocatorAsync(result)
                .ConfigureAwait(false))
                yield return nextResult;
        }

        public IAsyncEnumerable<QueryResult<JObject>> QueryAllAsync(string query)
        {
            return QueryAllAsync<JObject>(query);
        }

        public async IAsyncEnumerable<QueryResult<T>> QueryAllAsync<T>(string query)
        {
            var request = new CompositeRequest();
            request.QueryAll("q", query);
            var compositeResult = await Composite.PostAsync(request)
                .ConfigureAwait(false);
            compositeResult.Assert();
            var result = compositeResult.Queries<T>("q");
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

        public async Task<T?> QueryByIdAsync<T>(string objectName, string recordId) where T: class
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

        public Task<JObject?> ExecuteRestApiAsync(string apiName)
        {
            return ExecuteRestApiAsync<JObject>(apiName);
        }

        public Task<T?> ExecuteRestApiAsync<T>(string apiName) where T : class
        {
            if (string.IsNullOrEmpty(apiName))
                throw new ArgumentNullException(nameof(apiName));

            return JsonHttp.HttpGetRestApiAsync<T>(apiName);
        }

        public Task<JObject?> ExecuteRestApiAsync(string apiName, object inputObject)
        {
            return ExecuteRestApiAsync<JObject>(apiName, inputObject);
        }

        public Task<T?> ExecuteRestApiAsync<T>(string apiName, object inputObject) where T: class
        {
            if (string.IsNullOrEmpty(apiName)) throw new ArgumentNullException(nameof(apiName));
            if (inputObject == null) throw new ArgumentNullException(nameof(inputObject));

            return JsonHttp.HttpPostRestApiAsync<T>(apiName, inputObject);
        }

        public async Task<SuccessResponse> CreateAsync(string objectName, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (record == null) throw new ArgumentNullException(nameof(record));

            return await JsonHttp.HttpPostAsync<SuccessResponse>(record, $"sobjects/{objectName}")
                .ConfigureAwait(false) ?? new SuccessResponse();
        }

        public async Task<SaveResponse> CreateAsync(string objectName, CreateRequest request)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await Dnf.TryDeserializeObjectAsync(
                JsonHttp.HttpPostAsync<SaveResponse>(request, $"composite/tree/{objectName}"))
                .ConfigureAwait(false) ?? new SaveResponse();
        }

        public Task<SuccessResponse> UpdateAsync(string objectName, string recordId, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            return JsonHttp.HttpPatchAsync(record, $"sobjects/{objectName}/{recordId}");
        }

        public Task<SuccessResponse> UpsertExternalAsync(string objectName, string externalFieldName, string externalId, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            return JsonHttp.HttpPatchAsync(record, $"sobjects/{objectName}/{externalFieldName}/{externalId}");
        }

        public Task<SuccessResponse> UpsertExternalAsync(string objectName, string externalFieldName, string externalId, object record, bool ignoreNull)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            return JsonHttp.HttpPatchAsync(record, $"sobjects/{objectName}/{externalFieldName}/{externalId}", ignoreNull);
        }

        public Task<bool> DeleteAsync(string objectName, string recordId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));

            return JsonHttp.HttpDeleteAsync($"sobjects/{objectName}/{recordId}");
        }

        public Task<bool> DeleteExternalAsync(string objectName, string externalFieldName, string externalId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));

            return JsonHttp.HttpDeleteAsync($"sobjects/{objectName}/{externalFieldName}/{externalId}");
        }

        public Task<DescribeGlobalResult<JObject>?> GetObjectsAsync()
        {
            return GetObjectsAsync<JObject>();
        }

        public Task<DescribeGlobalResult<T>?> GetObjectsAsync<T>()
        {
            const string? resourceName = "sobjects";
            return JsonHttp.HttpGetAsync<DescribeGlobalResult<T>>(resourceName);
        }

        public Task<JObject?> BasicInformationAsync(string objectName)
        {
            return BasicInformationAsync<JObject>(objectName);
        }

        public Task<T?> BasicInformationAsync<T>(string objectName) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var resourceName = $"sobjects/{objectName}";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> DescribeAsync(string objectName)
        {
            return DescribeAsync<JObject>(objectName);
        }

        public Task<T?> DescribeAsync<T>(string objectName) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var resourceName = $"sobjects/{objectName}/describe/";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> GetDeleted(string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            return GetDeleted<JObject>(objectName, startDateTime, endDateTime);
        }

        public Task<T?> GetDeleted<T>(string objectName, DateTime startDateTime, DateTime endDateTime) where T : class
        {
            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));

            var resourceName = $"sobjects/{objectName}/deleted/?start={sdt}&end={edt}";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> GetUpdated(string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            return GetUpdated<JObject>(objectName, startDateTime, endDateTime);
        }

        public Task<T?> GetUpdated<T>(string objectName, DateTime startDateTime, DateTime endDateTime) where T : class
        {
            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));

            var resourceName = $"sobjects/{objectName}/updated/?start={sdt}&end={edt}";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> RecentAsync(int limit = 200)
        {
            return RecentAsync<JObject>(limit);
        }

        public Task<T?> RecentAsync<T>(int limit = 200) where T : class
        {
            var resourceName = $"recent/?limit={limit}";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public Task<IList<JObject>> SearchAsync(string query)
        {
            return SearchAsync<JObject>(query);
        }

        public async Task<IList<T>> SearchAsync<T>(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));
            if (!query.Contains("FIND")) throw new ArgumentException("query does not contain FIND");
            if (!query.Contains("{") || !query.Contains("}")) throw new ArgumentException("search term must be wrapped in braces");

            var resourceName = $"search?q={Dnf.EscapeDataString(query)}";
            return await JsonHttp.HttpGetAsync<IList<T>>(resourceName)
                .ConfigureAwait(false) ?? new List<T>();
        }

        public Task<JObject?> UserInfo()
        {
            return UserInfo<JObject>(Id);
        }

        public Task<T?> UserInfo<T>() where T : class
        {
            return UserInfo<T>(Id);
        }

        public Task<JObject?> UserInfo(string? url)
        {
            return UserInfo<JObject>(url);
        }

        public Task<T?> UserInfo<T>(string? url) where T : class
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) throw new FormatException("url");

            return JsonHttp.HttpGetAsync<T>(new Uri(url));
        }

        #endregion

        #region CRUD

        public Task<Stream> RetrieveBlobAsync(string objectName, string recordId, string blobField)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (string.IsNullOrEmpty(blobField)) throw new ArgumentNullException(nameof(blobField));

            var resourceName = $"sobjects/{objectName}/{recordId}/{blobField}";
            return JsonHttp.HttpGetBlobAsync(resourceName);
        }

        public Task<JObject?> RetrieveExternalAsync(string objectName, string externalFieldName, string externalId, params string[] fields)
        {
            return RetrieveExternalAsync<JObject>(objectName, externalFieldName, externalId, fields);
        }

        public Task<T?> RetrieveExternalAsync<T>(string objectName, string externalFieldName, string externalId, params string[] fields) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            var resourceName = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}";
            if (fields.Length > 0)
                resourceName += $"?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        public Task<Stream> RetrieveRichTextImageAsync(string objectName, string recordId, string fieldName, string contentReferenceId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (string.IsNullOrEmpty(contentReferenceId)) throw new ArgumentNullException(nameof(contentReferenceId));

            var resourceName = $"sobjects/{objectName}/{recordId}/richTextImageFields/{fieldName}/{contentReferenceId}";
            return JsonHttp.HttpGetBlobAsync(resourceName);
        }

        public Task<JObject?> RelationshipsAsync(string objectName, string recordId, string relationshipFieldName, string[]? fields = null)
        {
            if (objectName == null) throw new ArgumentNullException(nameof(objectName));
            if (recordId == null) throw new ArgumentNullException(nameof(recordId));
            if (relationshipFieldName == null) throw new ArgumentNullException(nameof(relationshipFieldName));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            return RelationshipsAsync<JObject>(objectName, recordId, relationshipFieldName);
        }

        public Task<T?> RelationshipsAsync<T>(string objectName, string recordId, string relationshipFieldName, string[]? fields = null) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (string.IsNullOrEmpty(relationshipFieldName)) throw new ArgumentNullException(nameof(relationshipFieldName));

            var resourceName = $"sobjects/{objectName}/{recordId}/{relationshipFieldName}";
            if (fields?.Length > 0) resourceName += $"?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}";
            return JsonHttp.HttpGetAsync<T>(resourceName);
        }

        #endregion
    }
}
