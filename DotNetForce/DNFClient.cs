using DotNetForce;
using DotNetForce.Chatter;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Models.Xml;
using DotNetForce.Force;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DotNetForce
{
    public partial class DNFClient : IForceClient// : IDisposable
    {
        public static string DefaultApiVersion = "v43.0";

        protected Uri LoginUri { get; set; }
        protected string ClientId { get; set; }
        protected string ClientSecret { get; set; }

        protected string Id { get; set; }
        protected string InstanceUrl { get; set; }
        protected string RefreshToken { get; set; }
        protected string AccessToken { get; set; }
        protected string ApiVersion { get; set; }

        public Action<string> Logger { get; set; }

        public int? ApiUsed { get => JsonHttp.ApiLastRetrieve < XmlHttp.ApiLastRetrieve ? XmlHttp.ApiUsed : JsonHttp.ApiUsed; }
        public int? ApiLimit { get => JsonHttp.ApiLastRetrieve < XmlHttp.ApiLastRetrieve ? XmlHttp.ApiLimit : JsonHttp.ApiLimit; }
        public int? PerAppApiUsed { get => JsonHttp.PerAppApiLastRetrieve < XmlHttp.PerAppApiLastRetrieve ? XmlHttp.PerAppApiUsed : JsonHttp.PerAppApiUsed; }
        public int? PerAppApiLimit { get => JsonHttp.PerAppApiLastRetrieve < XmlHttp.PerAppApiLastRetrieve ? XmlHttp.PerAppApiLimit : JsonHttp.PerAppApiLimit; }

        #region Client

        public JsonHttpClient JsonHttp { get; set; }

        public XmlHttpClient XmlHttp { get; set; }

        public IChatterClient Chatter { get; set; }

        public ICompositeClient Composite { get; set; }

        public IToolingClient Tooling { get; set; }

        protected IForceClient Force { get; set; }

        #endregion

        protected DNFClient() { }

        #region Login

        public static Task<DNFClient> LoginAsync(Uri loginUri, string clientId, string clientSecret, string userName, string password)
        {
            return LoginAsync(loginUri, clientId, clientSecret, userName, password, null);
        }

        public static async Task<DNFClient> LoginAsync(Uri loginUri, string clientId, string clientSecret, string userName, string password, Action<string> logger)
        {
            var client = new DNFClient { LoginUri = loginUri, ClientId = clientId, ClientSecret = clientSecret, Logger = logger };

            client.Logger?.Invoke($"DNFClient connecting...");
            var timer = Stopwatch.StartNew();

            using (var auth = new AuthenticationClient() { ApiVersion = DefaultApiVersion })
            {
                var tokenRequestEndpointUrl = new Uri(new Uri(client.LoginUri.GetLeftPart(UriPartial.Authority)), "/services/oauth2/token").ToString();
                await auth.UsernamePasswordAsync(clientId, clientSecret, userName, password, tokenRequestEndpointUrl).ConfigureAwait(false);
                var httpHandler = new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };
                var jsonClient = new HttpClient(httpHandler) { Timeout = TimeSpan.FromSeconds(60 * 30) };
                //jsonClient.DefaultRequestHeaders.ConnectionClose = true;
                jsonClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                var xmlClient = new HttpClient(httpHandler) { Timeout = TimeSpan.FromSeconds(60 * 30) };
                //xmlClient.DefaultRequestHeaders.ConnectionClose = true;
                xmlClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                client.Id = auth.Id;
                client.InstanceUrl = auth.InstanceUrl;
                client.RefreshToken = auth.RefreshToken;
                client.AccessToken = auth.AccessToken;
                client.ApiVersion = auth.ApiVersion;

                client.JsonHttp = new JsonHttpClient(client.InstanceUrl, client.ApiVersion, client.AccessToken, jsonClient);
                client.XmlHttp = new XmlHttpClient(client.InstanceUrl, client.ApiVersion, client.AccessToken, xmlClient);

                client.Force = new ForceClient(client.InstanceUrl, client.AccessToken, client.ApiVersion, client.JsonHttp, client.XmlHttp);
                client.Chatter = new ChatterClient(client.InstanceUrl, client.AccessToken, client.ApiVersion, client.JsonHttp);
                client.Composite = new CompositeClient(client.JsonHttp, client.ApiVersion, client.Logger);
                client.Tooling = new ToolingClient(client.JsonHttp);
            }
            client.Logger?.Invoke($"DNFClient connected ({timer.Elapsed.TotalSeconds} seconds)");
            return client;
        }

        public static Task<DNFClient> OAuthLoginAsync(OAuthProfile oAuthProfile)
        {
            return OAuthLoginAsync(oAuthProfile, null);
        }

        public static async Task<DNFClient> OAuthLoginAsync(OAuthProfile oAuthProfile, Action<string> logger)
        {
            var client = new DNFClient { LoginUri = oAuthProfile.LoginUri, ClientId = oAuthProfile.ClientId, ClientSecret = oAuthProfile.ClientSecret, Logger = logger };

            client.Logger?.Invoke($"DNFClient connecting...");
            var timer = Stopwatch.StartNew();

            using (var auth = new AuthenticationClient() { ApiVersion = DefaultApiVersion })
            {
                var tokenRequestEndpointUrl = new Uri(new Uri(client.LoginUri.GetLeftPart(UriPartial.Authority)), "/services/oauth2/token").ToString();
                await auth.WebServerAsync(oAuthProfile.ClientId, oAuthProfile.ClientSecret, oAuthProfile.RedirectUri, oAuthProfile.Code, tokenRequestEndpointUrl).ConfigureAwait(false);
                var httpHandler = new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };
                var jsonClient = new HttpClient(httpHandler) { Timeout = TimeSpan.FromSeconds(60 * 30) };
                //jsonClient.DefaultRequestHeaders.ConnectionClose = true;
                jsonClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                var xmlClient = new HttpClient(httpHandler) { Timeout = TimeSpan.FromSeconds(60 * 30) };
                //xmlClient.DefaultRequestHeaders.ConnectionClose = true;
                xmlClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                client.Id = auth.Id;
                client.InstanceUrl = auth.InstanceUrl;
                client.RefreshToken = auth.RefreshToken;
                client.AccessToken = auth.AccessToken;
                client.ApiVersion = auth.ApiVersion;

                client.JsonHttp = new JsonHttpClient(client.InstanceUrl, client.ApiVersion, client.AccessToken, jsonClient);
                client.XmlHttp = new XmlHttpClient(client.InstanceUrl, client.ApiVersion, client.AccessToken, xmlClient);

                client.Force = new ForceClient(client.InstanceUrl, client.AccessToken, client.ApiVersion, jsonClient, xmlClient);
                client.Chatter = new ChatterClient(client.InstanceUrl, client.AccessToken, client.ApiVersion, jsonClient);
                client.Composite = new CompositeClient(client.JsonHttp, client.ApiVersion, client.Logger);
                client.Tooling = new ToolingClient(client.JsonHttp);
            }
            client.Logger?.Invoke($"DNFClient connected ({timer.Elapsed.TotalSeconds} seconds)");
            return client;
        }

        public async Task TokenRefreshAsync()
        {
            using (var auth = new AuthenticationClient() { ApiVersion = ApiVersion })
            {
                var tokenRequestEndpointUrl = new Uri(new Uri(LoginUri.GetLeftPart(UriPartial.Authority)), "/services/oauth2/token").ToString();
                await auth.TokenRefreshAsync(ClientId, RefreshToken, ClientSecret, tokenRequestEndpointUrl).ConfigureAwait(false);

                Id = auth.Id;
                InstanceUrl = auth.InstanceUrl;
                AccessToken = auth.AccessToken;
            }
        }

        #endregion



        #region STANDARD
        public Task<QueryResult<JObject>> QueryAsync(string query) => QueryAsync<JObject>(query);
        public async Task<QueryResult<T>> QueryAsync<T>(string query)
        {
            var request = new CompositeRequest();
            request.Query("q", query);
            var result = await Composite.PostAsync(request).ConfigureAwait(false);
            DNF.ThrowIfError(result);
            return result.Queries<T>("q");
        }
        public async Task<IEnumerable<JObject>> GetEnumerableAsync(string query) => GetEnumerable(await QueryAsync<JObject>(query));
        public async Task<IEnumerable<T>> GetEnumerableAsync<T>(string query) => GetEnumerable(await QueryAsync<T>(query));
        public async Task<IEnumerable<JObject>> GetLazyEnumerableAsync(string query) => GetLazyEnumerable(await QueryAsync<JObject>(query));
        public async Task<IEnumerable<T>> GetLazyEnumerableAsync<T>(string query) => GetLazyEnumerable(await QueryAsync<T>(query));

        public Task<QueryResult<JObject>> QueryContinuationAsync(string nextRecordsUrl) => QueryContinuationAsync<JObject>(nextRecordsUrl);
        public Task<QueryResult<T>> QueryContinuationAsync<T>(string nextRecordsUrl) => Force.QueryContinuationAsync<T>(nextRecordsUrl);

        public Task<QueryResult<JObject>> QueryAllAsync(string query) => QueryAllAsync<JObject>(query);
        public async Task<QueryResult<T>> QueryAllAsync<T>(string query)
        {
            var request = new CompositeRequest();
            request.QueryAll("q", query);
            var result = await Composite.PostAsync(request).ConfigureAwait(false);
            DNF.ThrowIfError(result);
            return result.Results("q").ToObject<QueryResult<T>>();
        }
        public async Task<IEnumerable<JObject>> GetAllEnumerableAsync(string query) => GetEnumerable(await QueryAllAsync<JObject>(query));
        public async Task<IEnumerable<T>> GetAllEnumerableAsync<T>(string query) => GetEnumerable(await QueryAllAsync<T>(query));
        public async Task<IEnumerable<JObject>> GetAllLazyEnumerableAsync(string query) => GetLazyEnumerable(await QueryAllAsync<JObject>(query));
        public async Task<IEnumerable<T>> GetAllLazyEnumerableAsync<T>(string query) => GetLazyEnumerable(await QueryAllAsync<T>(query));

        public Task<JObject> QueryByIdAsync(string objectName, string recordId) => QueryByIdAsync<JObject>(objectName, recordId);
        public Task<T> QueryByIdAsync<T>(string objectName, string recordId) => Force.QueryByIdAsync<T>(objectName, recordId);
        public Task<JObject> ExecuteRestApiAsync(string apiName) => ExecuteRestApiAsync<JObject>(apiName);
        public Task<T> ExecuteRestApiAsync<T>(string apiName) => Force.ExecuteRestApiAsync<T>(apiName);
        public Task<JObject> ExecuteRestApiAsync(string apiName, object inputObject) => ExecuteRestApiAsync<JObject>(apiName, inputObject);
        public Task<T> ExecuteRestApiAsync<T>(string apiName, object inputObject) => Force.ExecuteRestApiAsync<T>(apiName, inputObject);
        public Task<SuccessResponse> CreateAsync(string objectName, object record) => Force.CreateAsync(objectName, record);
        public Task<SaveResponse> CreateAsync(string objectName, CreateRequest request) => Force.CreateAsync(objectName, request);
        public Task<SuccessResponse> UpdateAsync(string objectName, string recordId, object record) => Force.UpdateAsync(objectName, recordId, record);
        public Task<SuccessResponse> UpsertExternalAsync(string objectName, string externalFieldName, string externalId, object record) => Force.UpsertExternalAsync(objectName, externalFieldName, HttpUtility.UrlEncode(externalId), record);
        public Task<SuccessResponse> UpsertExternalAsync(string objectName, string externalFieldName, string externalId, object record, bool ignoreNull) => Force.UpsertExternalAsync(objectName, externalFieldName, HttpUtility.UrlEncode(externalId), record, ignoreNull);
        public Task<bool> DeleteAsync(string objectName, string recordId) => Force.DeleteAsync(objectName, recordId);
        public Task<bool> DeleteExternalAsync(string objectName, string externalFieldName, string externalId) => Force.DeleteExternalAsync(objectName, externalFieldName, HttpUtility.UrlEncode(externalId));
        public Task<DescribeGlobalResult<JObject>> GetObjectsAsync() => GetObjectsAsync<JObject>();
        public Task<DescribeGlobalResult<T>> GetObjectsAsync<T>() => Force.GetObjectsAsync<T>();
        public Task<JObject> BasicInformationAsync(string objectName) => BasicInformationAsync<JObject>(objectName);
        public Task<T> BasicInformationAsync<T>(string objectName) => Force.BasicInformationAsync<T>(objectName);
        public Task<JObject> DescribeAsync(string objectName) => DescribeAsync<JObject>(objectName);
        public Task<T> DescribeAsync<T>(string objectName) => Force.DescribeAsync<T>(objectName);
        public Task<JObject> GetDeleted(string objectName, DateTime startDateTime, DateTime endDateTime) => GetDeleted<JObject>(objectName, startDateTime, endDateTime);
        public Task<T> GetDeleted<T>(string objectName, DateTime startDateTime, DateTime endDateTime) => Force.GetDeleted<T>(objectName, startDateTime, endDateTime);
        public Task<JObject> GetUpdated(string objectName, DateTime startDateTime, DateTime endDateTime) => GetUpdated<JObject>(objectName, startDateTime, endDateTime);
        public Task<T> GetUpdated<T>(string objectName, DateTime startDateTime, DateTime endDateTime) => Force.GetUpdated<T>(objectName, startDateTime, endDateTime);
        public Task<JObject> DescribeLayoutAsync(string objectName) => DescribeLayoutAsync<JObject>(objectName);
        public Task<T> DescribeLayoutAsync<T>(string objectName) => Force.DescribeLayoutAsync<T>(objectName);
        public Task<JObject> DescribeLayoutAsync(string objectName, string recordTypeId) => DescribeLayoutAsync<JObject>(objectName, recordTypeId);
        public Task<T> DescribeLayoutAsync<T>(string objectName, string recordTypeId) => Force.DescribeLayoutAsync<T>(objectName, recordTypeId);
        public Task<JObject> RecentAsync(int limit = 200) => RecentAsync<JObject>(limit);
        public Task<T> RecentAsync<T>(int limit = 200) => Force.RecentAsync<T>(limit);
        public Task<List<JObject>> SearchAsync(string query) => SearchAsync<JObject>(query);
        public Task<List<T>> SearchAsync<T>(string query) => Force.SearchAsync<T>(query);
        public Task<JObject> UserInfo(string url) => UserInfo<JObject>(url);
        public Task<T> UserInfo<T>(string url) => Force.UserInfo<T>(url);
        #endregion

        #region BULK
        public Task<List<BatchInfoResult>> RunJobAsync<T>(string objectName, BulkConstants.OperationType operationType, IEnumerable<ISObjectList<T>> recordsLists) => Force.RunJobAsync<T>(objectName, operationType, recordsLists);
        public Task<List<BatchResultList>> RunJobAndPollAsync<T>(string objectName, BulkConstants.OperationType operationType, IEnumerable<ISObjectList<T>> recordsLists) => Force.RunJobAndPollAsync<T>(objectName, operationType, recordsLists);
        public Task<JobInfoResult> CreateJobAsync(string objectName, BulkConstants.OperationType operationType) => Force.CreateJobAsync(objectName, operationType);
        public Task<BatchInfoResult> CreateJobBatchAsync<T>(JobInfoResult jobInfo, ISObjectList<T> recordsObject) => Force.CreateJobBatchAsync<T>(jobInfo, recordsObject);
        public Task<BatchInfoResult> CreateJobBatchAsync<T>(string jobId, ISObjectList<T> recordsObject) => Force.CreateJobBatchAsync<T>(jobId, recordsObject);
        public Task<JobInfoResult> CloseJobAsync(JobInfoResult jobInfo) => Force.CloseJobAsync(jobInfo);
        public Task<JobInfoResult> CloseJobAsync(string jobId) => Force.CloseJobAsync(jobId);
        public Task<JobInfoResult> PollJobAsync(JobInfoResult jobInfo) => Force.PollJobAsync(jobInfo);
        public Task<JobInfoResult> PollJobAsync(string jobId) => Force.PollJobAsync(jobId);
        public Task<BatchInfoResult> PollBatchAsync(BatchInfoResult batchInfo) => Force.PollBatchAsync(batchInfo);
        public Task<BatchInfoResult> PollBatchAsync(string batchId, string jobId) => Force.PollBatchAsync(batchId, jobId);
        public Task<BatchResultList> GetBatchResultAsync(BatchInfoResult batchInfo) => Force.GetBatchResultAsync(batchInfo);
        public Task<BatchResultList> GetBatchResultAsync(string batchId, string jobId) => Force.GetBatchResultAsync(batchId, jobId);
        #endregion

        public Task<JObject> LimitsAsync() => LimitsAsync<JObject>();
        public async Task<T> LimitsAsync<T>()
        {
            return await JsonHttp.HttpGetAsync<T>("limits").ConfigureAwait(false);
        }

        public async Task<int> DailyApiUsed()
        {
            var limits = await LimitsAsync<JObject>();
            return (int?)limits?["DailyApiRequests"]?["Remaining"] ?? 0;
        }

        public async Task<int> DailyApiLimit()
        {
            var limits = await LimitsAsync<JObject>();
            return (int?)limits?["DailyApiRequests"]?["Max"] ?? 0;
        }

        #region sobjects

        public Task<JObject> NamedLayoutsAync(string objectName, string layoutName) => NamedLayoutsAync<JObject>(objectName, layoutName);
        public async Task<T> NamedLayoutsAync<T>(string objectName, string layoutName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(layoutName)) throw new ArgumentNullException("layoutName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/describe/namedLayouts/{layoutName}").ConfigureAwait(false);
        }

        public Task<JObject> ApprovalLayoutsAync(string objectName, string approvalProcessName = "") => ApprovalLayoutsAync<JObject>(objectName, approvalProcessName);
        public async Task<T> ApprovalLayoutsAync<T>(string objectName, string approvalProcessName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");


            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/describe/approvalLayouts/{approvalProcessName}").ConfigureAwait(false);
        }

        public Task<JObject> CompactLayoutsAync(string objectName) => CompactLayoutsAync<JObject>(objectName);
        public async Task<T> CompactLayoutsAync<T>(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/describe/compactLayouts/").ConfigureAwait(false);
        }

        public Task<JObject> DescribeLayoutsAync(string objectName = "Global") => DescribeLayoutAsync(objectName);
        public async Task<T> DescribeLayoutsAync<T>(string objectName = "Global")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/describe/layouts/").ConfigureAwait(false);
        }

        public Task<JObject> PlatformActionAync() => PlatformActionAync<JObject>();
        public async Task<T> PlatformActionAync<T>()
        {
            return await JsonHttp.HttpGetAsync<T>($"sobjects/PlatformAction").ConfigureAwait(false);
        }

        public Task<JObject> QuickActionsAync(string objectName, string actionName = "") => QuickActionsAync<JObject>(objectName, actionName);
        public async Task<T> QuickActionsAync<T>(string objectName, string actionName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/quickActions/{actionName}").ConfigureAwait(false);
        }

        public Task<JObject> QuickActionsDetailsAync(string objectName, string actionName) => QuickActionsDetailsAync<JObject>(objectName, actionName);
        public async Task<T> QuickActionsDetailsAync<T>(string objectName, string actionName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException("actionName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/quickActions/{actionName}/describe/").ConfigureAwait(false);
        }

        public Task<JObject> QuickActionsDefaultValuesAync(string objectName, string actionName, string contextId) => QuickActionsDefaultValuesAync<JObject>(objectName, actionName, contextId);
        public async Task<T> QuickActionsDefaultValuesAync<T>(string objectName, string actionName, string contextId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException("actionName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/quickActions/{actionName}/defaultValues/{contextId}").ConfigureAwait(false);
        }

        #endregion

        #region CRUD

        public async Task<System.IO.Stream> BlobRetrieveAync(string objectName, string recordId, string blobField)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (string.IsNullOrEmpty(blobField)) throw new ArgumentNullException("blobField");

            return await JsonHttp.HttpGetBlobAsync($"sobjects/{objectName}/{recordId}/{blobField}").ConfigureAwait(false);
        }

        public Task<JObject> RetrieveExternalAsync(string objectName, string externalFieldName, string externalId, params string[] fields) => RetrieveExternalAsync(objectName, externalFieldName, externalId, fields);
        public async Task<T> RetrieveExternalAsync<T>(string objectName, string externalFieldName, string externalId, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException("externalId");
            if (fields == null) throw new ArgumentNullException("fields");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/{externalFieldName}/{HttpUtility.UrlEncode(externalId)}" +
                (fields?.Length > 0 ? $"?fields={string.Join(",", fields.Select(field => HttpUtility.UrlEncode(field)))}" : "")).ConfigureAwait(false);
        }

        public async Task<System.IO.Stream> RichTextImageRetrieveAsync(string objectName, string recordId, string fieldName, string contentReferenceId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException("fieldName");
            if (string.IsNullOrEmpty(contentReferenceId)) throw new ArgumentNullException("contentReferenceId");

            return await JsonHttp.HttpGetBlobAsync($"sobjects/{objectName}/{recordId}/richTextImageFields/{fieldName}/{contentReferenceId}").ConfigureAwait(false);
        }

        public Task<JObject> RelationshipsAync(string objectName, string recordId, string relationshipFieldName, string[] fields = null) => RelationshipsAync(objectName, recordId, relationshipFieldName, fields = null);
        public async Task<T> RelationshipsAync<T>(string objectName, string recordId, string relationshipFieldName, string[] fields = null)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (string.IsNullOrEmpty(relationshipFieldName)) throw new ArgumentNullException("relationshipFieldName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/{recordId}/{relationshipFieldName}" +
                (fields?.Length > 0 ? $"?fields={string.Join(",", fields.Select(field => HttpUtility.UrlEncode(field)))}" : "")).ConfigureAwait(false);
        }

        #endregion

        public async Task<UserInfo> UserInfoAsync()
        {
            return await Force.UserInfo<UserInfo>(Id).ConfigureAwait(false);
        }

        public Task<List<JObject>> VersionsAsync() => VersionsAsync<JObject>();
        public async Task<List<T>> VersionsAsync<T>()
        {
            return await JsonHttp.HttpGetAsync<List<T>>(new Uri(new Uri(InstanceUrl), "/services/data")).ConfigureAwait(false);
        }

        public Task<JObject> ResourcesAsync() => ResourcesAsync<JObject>(ApiVersion);
        public Task<T> ResourcesAsync<T>() => ResourcesAsync<T>(ApiVersion);
        public Task<JObject> ResourcesAsync(string apiVersion) => ResourcesAsync<JObject>(apiVersion);
        public async Task<T> ResourcesAsync<T>(string apiVersion)
        {
            if (string.IsNullOrEmpty(apiVersion)) throw new ArgumentNullException("apiVersion");

            return await JsonHttp.HttpGetAsync<T>(new Uri(new Uri(InstanceUrl), $"/services/data/{apiVersion}")).ConfigureAwait(false);
        }

        //public IEnumerable<JObject> GetEnumerable(JObject parent, string field)
        //{
        //    //if (parent == null) throw new ArgumentNullException("parent");
        //    if (parent == null) return Enumerable.Empty<JObject>();

        //    if (parent[field] == null || parent[field].Type == JTokenType.Null)
        //    {
        //        return Enumerable.Empty<JObject>();
        //    }

        //    return GetEnumerable(parent[field].ToObject<QueryResult<JObject>>());
        //}

        //public IEnumerable<TChild> GetEnumerable<T, TChild>(T parent, string field)
        //{
        //    if (parent == null) throw new ArgumentNullException("parent");

        //    var objParent = JObject.FromObject(parent);

        //    if (objParent[field] == null || objParent[field].Type == JTokenType.Null)
        //    {
        //        return Enumerable.Empty<TChild>();
        //    }

        //    return GetEnumerable(objParent[field].ToObject<QueryResult<TChild>>());
        //}

        //public IEnumerable<TChild> ToLazyEnumerable<T, TChild>(T parent, string field)
        //{
        //    if (parent == null) throw new ArgumentNullException("parent");

        //    var objParent = JObject.FromObject(parent);

        //    if (objParent[field] == null || objParent[field].Type == JTokenType.Null)
        //    {
        //        return Enumerable.Empty<TChild>();
        //    }

        //    return ToLazyEnumerable(objParent[field].ToObject<QueryResult<TChild>>());
        //}

        public IEnumerable<T> GetEnumerable<T>(QueryResult<T> queryResult)
        {
            if (queryResult == null)
            {
                yield break;
            }

            var batchSize = GetBatchSize(queryResult.NextRecordsUrl);

            foreach (var row in queryResult.Records)
            {
                //subscribe.OnNext(await IncludeRelationships(row).ConfigureAwait(false));
                yield return row;
            }

            if (queryResult.Records.Count < queryResult.TotalSize)
            {
                foreach (var row in QueryByBatchs(queryResult, batchSize))
                {
                    //subscribe.OnNext(await IncludeRelationships(row).ConfigureAwait(false));
                    yield return row;
                }
            }
        }

        //public async Task<T> IncludeRelationships<T>(T obj)
        //{
        //    var token = JObject.FromObject(obj);
        //    var batchs = new List<(int offset, int size, string nextUrl, string referenceId)>();

        //    foreach (var prop in token.Properties())
        //    {
        //        if (DNF.IsQueryResult(prop.Value))
        //        {
        //            var recordsCount = ((JArray)prop.Value["records"]).Count;
        //            var totalSize = (int)prop.Value["totalSize"];

        //            if (recordsCount < totalSize)
        //            {
        //                var nextRecordsUrl = prop.Value["nextRecordsUrl"]?.ToString();
        //                var batchSize = GetBatchSize(nextRecordsUrl);

        //                var remaining = totalSize - recordsCount;
        //                var subBatchSize = Math.Min(remaining, Math.Max(200, recordsCount));
        //                batchs.AddRange(GetQueryBatchs(nextRecordsUrl, recordsCount, remaining, subBatchSize, $"{prop.Name}-{{0}}-{{1}}"));
        //            }
        //        }
        //    }

        //    if (batchs.Count > 0)
        //    {
        //        var batchChunks = DNF.Chunk(batchs, DNF.COMPOSITE_QUERY_LIMIT);

        //        var tasks = new List<Task>();

        //        foreach (var (batch, batchIdx) in batchChunks.Select((batch, batchIdx) => (batch, batchIdx)))
        //        {
        //            tasks.Add(Task.Run(async () =>
        //            {
        //                await DNF.QueryCursorThrottler.WaitAsync().ConfigureAwait(true);
        //                try
        //                {
        //                    foreach (var (record, recordIdx) in QueryByBatch<T>(batch).Select((record, recordIdx) => (record, recordIdx)))
        //                    {
        //                        if (batchIdx > 0 && recordIdx == 0)
        //                        {
        //                            await tasks[batchIdx - 1].ConfigureAwait(false);
        //                        }
        //                        var propName = record.referenceId.Split('-').First();
        //                        var records = (JArray)token[propName]["records"];
        //                        records.Add(record.record);

        //                        if (records.Count >= (int?)token[propName]["totalSize"])
        //                        {
        //                            token[propName]["done"] = true;
        //                            token[propName]["nextRecordsUrl"] = null;
        //                        }
        //                    }
        //                }
        //                finally
        //                {
        //                    DNF.QueryCursorThrottler.Release();
        //                }
        //            }));
        //        }

        //        await Task.WhenAll(tasks).ConfigureAwait(false);
        //    }
        //    return token.ToObject<T>();
        //}

        public IEnumerable<T> GetLazyEnumerable<T>(QueryResult<T> queryResult)
        {
            foreach (var row in queryResult.Records)
            {
                yield return row;
            }

            var loaded = queryResult.Records.Count;
            var nextRecordsUrl = queryResult.NextRecordsUrl;

            while (loaded < queryResult.TotalSize && !string.IsNullOrEmpty(nextRecordsUrl))
            {
                foreach (var nextResult in Observable.Create<QueryResult<T>>(subscribe => Task.Run(async () =>
                {
                    try
                    {
                        var result = await QueryContinuationAsync<T>(nextRecordsUrl).ConfigureAwait(false);
                        subscribe.OnNext(result);
                        subscribe.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        subscribe.OnError(ex);
                    }
                    return default(T);
                })).ToEnumerable())
                {
                    foreach (var row in nextResult.Records)
                    {
                        if (++loaded <= queryResult.TotalSize)
                        {
                            yield return row;
                        }
                    }
                    nextRecordsUrl = nextResult.NextRecordsUrl;
                }
            }
        }

        private async Task QueryContinuationAsync<T>(QueryResult<T> queryResult, IObserver<T> subscribe)
        {
            var nextResult = queryResult;

            while (!string.IsNullOrEmpty(nextResult.NextRecordsUrl))
            {
                Logger?.Invoke($"Query Start\t{nextResult.NextRecordsUrl}");
                nextResult = await Force.QueryContinuationAsync<T>(nextResult.NextRecordsUrl).ConfigureAwait(false);
                Logger?.Invoke($"Query End\t{nextResult.NextRecordsUrl}");

                foreach (var row in nextResult.Records)
                {
                    //queryResult.Records.Add(row);
                    //queryResult.Done = queryResult.Records.Count >= queryResult.TotalSize;
                    subscribe.OnNext(row);
                }
            }
        }

        private IEnumerable<T> QueryByBatchs<T>(QueryResult<T> queryResult, int batchSize)
        {
            return Observable.Create<T>(subscribe => Task.Run(async () =>
            {
                try
                {
                    // batch size can be changed via Sforce-Query-Options header
                    // i.e. request.Query(referenceId, query).HttpHeaders = new HttpHeaders().QueryOptions(200);
                    // https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/headers_queryoptions.htm
                    // The default is 2,000; the minimum is 200, and the maximum is 2,000.
                    // There is no guarantee that the requested batch size is the actual batch size.
                    // Changes are made as necessary to maximize performance.
                    var batchs = GetQueryBatchs(queryResult.NextRecordsUrl, queryResult.Records.Count, queryResult.TotalSize - queryResult.Records.Count, batchSize);
                    var batchChunks = DNF.Chunk(batchs, DNF.COMPOSITE_QUERY_LIMIT);

                    var tasks = new List<Task>();

                    foreach (var (batch, batchIdx) in batchChunks.Select((batch, batchIdx) => (batch, batchIdx)))
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            //await DNF.QueryCursorThrottler.WaitAsync().ConfigureAwait(true);
                            //try
                            //{
                            foreach (var (record, recordIdx) in QueryByBatch<T>(batch).Select((record, recordIdx) => (record, recordIdx)))
                            {
                                if (batchIdx > 0 && recordIdx == 0)
                                {
                                    await tasks[batchIdx - 1].ConfigureAwait(false);
                                }
                                //queryResult.Records.Add(record);
                                //queryResult.Done = queryResult.Records.Count >= queryResult.TotalSize;
                                subscribe.OnNext(record.record);
                            }
                            //}
                            //finally
                            //{
                            //    DNF.QueryCursorThrottler.Release();
                            //}
                        }));
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    subscribe.OnCompleted();
                }
                catch (Exception ex)
                {
                    subscribe.OnError(ex);
                }
            })).ToEnumerable();
        }

        private IEnumerable<(T record, string referenceId)> QueryByBatch<T>(List<(int offset, int size, string nextUrl, string referenceId)> batchChunk)
        {
            return Observable.Create<(T record, string referenceId)>(subscribe => Task.Run(async () =>
            {
                try
                {
                    var request = new CompositeRequest();

                    foreach (var (offset, size, nextUrl, referenceId) in batchChunk)
                    {
                        request.CompositeRequests.Add(new CompositeSubrequest
                        {
                            ReferenceId = referenceId,
                            ResponseType = "query",
                            Method = "GET",
                            Url = nextUrl
                        });
                    }

                    Logger?.Invoke($@"Query Start {JsonConvert.SerializeObject(request.CompositeRequests)}");
                    var result = await Composite.PostAsync(request).ConfigureAwait(false);

                    DNF.ThrowIfError(result);

                    Logger?.Invoke($@"Query End {JsonConvert.SerializeObject(result.Queries().Select(que => new
                    {
                        ReferenceId = que.Key,
                        que.Value.Done,
                        que.Value.TotalSize,
                        que.Value.NextRecordsUrl,
                        RecordsCount = que.Value.Records.Count
                    }))}");

                    foreach (var (offset, size, nextUrl, referenceId) in batchChunk)
                    {
                        var query = result.Queries(referenceId);

                        if (query == null || query.Records?.Any() != true)
                        {
                            throw new ForceException(Error.Unknown, result.Errors(referenceId)?.ToString() ?? "GetEnumerable Failed.");
                        }

                        // actual batch size can be more than requested batch size
                        var records = query.Records.Take(size).Cast<T>().ToList();

                        foreach (var record in records)
                        {
                            subscribe.OnNext((record, referenceId));
                        }

                        // actual batch size can be less than requested batch size
                        if (records.Count < size)
                        {
                            var remaining = size - records.Count;
                            var subBatchSize = Math.Min(remaining, Math.Max(200, records.Count));
                            var subBatchs = GetQueryBatchs(nextUrl, offset + records.Count, remaining, subBatchSize);
                            foreach (var batchResult in QueryByBatch<T>(subBatchs))
                            {
                                subscribe.OnNext(batchResult);
                            }
                        }
                    }

                    subscribe.OnCompleted();
                }
                catch (Exception ex)
                {
                    subscribe.OnError(ex);
                }
            })).ToEnumerable();
        }


        private int GetBatchSize(string url)
        {
            if (url == null) return 2000;
            url = Regex.Replace(url, @"^/services/data/[^/]+/", "");
            return !int.TryParse(Regex.Match(url ?? "", @"^(?:.*query/01g[^/]+)-(\d+)$").Groups[1].Value, out int intVal)
                ? 2000 : intVal < 1 ? 1 : intVal > 2000 ? 2000 : intVal;
        }

        private string GetNextUrl(string url, int batchNo)
        {
            if (url == null) return null;
            url = Regex.Replace(url, @"^/services/data/[^/]+/", "");
            return Regex.Replace(url ?? "", @"^(.*query/[^/]+)-\d+$", $"$1-{batchNo}");
        }

        private List<(int offset, int size, string nextUrl, string referenceId)> GetQueryBatchs(string nextRecordsUrl, int loaded, int remaining, int batchSize)
        {
            return GetQueryBatchs(nextRecordsUrl, loaded, remaining, batchSize, "{0}-{1}");
        }

        private List<(int offset, int size, string nextUrl, string referenceId)> GetQueryBatchs(string nextRecordsUrl, int loaded, int remaining, int batchSize, string referenceFormat)
        {
            var noOfBatch = (int)Math.Ceiling(remaining / (double)batchSize);
            var batchs = Enumerable.Range(0, noOfBatch).Select(i =>
            {
                var offset = loaded + i * batchSize;
                var last = Math.Min(loaded + remaining, offset + batchSize);
                var size = last - offset;
                var nextUrl = GetNextUrl(nextRecordsUrl, offset);
                var referenceId = string.Format(referenceFormat, offset, size);
                return (offset, size, nextUrl, referenceId);
            }).ToList();
            return batchs;
        }

        public void Dispose()
        {
            JsonHttp.Dispose();
            XmlHttp.Dispose();
            Force.Dispose();
        }
    }
}
