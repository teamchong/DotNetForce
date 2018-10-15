using DotNetForce;
using DotNetForce.Chatter;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Models.Xml;
using DotNetForce.Force;
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

        public int? ApiUsed { get => new[] { JsonHttp?.ApiUsed, XmlHttp?.ApiUsed }.Max(); }
        public int? ApiLimit { get => new[] { JsonHttp?.ApiLimit, XmlHttp?.ApiLimit }.Max(); }
        public int? PerAppApiUsed { get => new[] { JsonHttp?.PerAppApiUsed, XmlHttp?.PerAppApiUsed }.Max(); }
        public int? PerAppApiLimit { get => new[] { JsonHttp?.PerAppApiLimit, XmlHttp?.PerAppApiLimit }.Max(); }

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
                var jsonClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60 * 30) };
                jsonClient.DefaultRequestHeaders.ConnectionClose = true;
                var xmlClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60 * 30) };
                xmlClient.DefaultRequestHeaders.ConnectionClose = true;
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

        public static Task<DNFClient> LoginAsync(OAuthProfile oAuthProfile)
        {
            return LoginAsync(oAuthProfile, null);
        }

        public static async Task<DNFClient> LoginAsync(OAuthProfile oAuthProfile, Action<string> logger)
        {
            var client = new DNFClient { LoginUri = oAuthProfile.LoginUri, ClientId = oAuthProfile.ClientId, ClientSecret = oAuthProfile.ClientSecret, Logger = logger };

            client.Logger?.Invoke($"DNFClient connecting...");
            var timer = Stopwatch.StartNew();

            using (var auth = new AuthenticationClient() { ApiVersion = DefaultApiVersion })
            {
                var requestEndpointUrl = new Uri(new Uri(client.LoginUri.GetLeftPart(UriPartial.Authority)), "/services/oauth2/token").ToString();
                await auth.WebServerAsync(oAuthProfile.ClientId, oAuthProfile.ClientSecret, oAuthProfile.RedirectUri, oAuthProfile.Code, requestEndpointUrl).ConfigureAwait(false);
                var jsonClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60 * 30) };
                jsonClient.DefaultRequestHeaders.ConnectionClose = true;
                var xmlClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60 * 30) };
                xmlClient.DefaultRequestHeaders.ConnectionClose = true;
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
        public Task<QueryResult<T>> QueryAsync<T>(string query) => Force.QueryAsync<T>(query);
        public Task<QueryResult<T>> QueryContinuationAsync<T>(string nextRecordsUrl) => Force.QueryContinuationAsync<T>(nextRecordsUrl);
        public Task<QueryResult<T>> QueryAllAsync<T>(string query) => Force.QueryAllAsync<T>(query);
        public Task<T> QueryByIdAsync<T>(string objectName, string recordId) => Force.QueryByIdAsync<T>(objectName, recordId);
        public Task<T> ExecuteRestApiAsync<T>(string apiName) => Force.ExecuteRestApiAsync<T>(apiName);
        public Task<T> ExecuteRestApiAsync<T>(string apiName, object inputObject) => Force.ExecuteRestApiAsync<T>(apiName, inputObject);
        public Task<SuccessResponse> CreateAsync(string objectName, object record) => Force.CreateAsync(objectName, record);
        public Task<SaveResponse> CreateAsync(string objectName, CreateRequest request) => Force.CreateAsync(objectName, request);
        public Task<SuccessResponse> UpdateAsync(string objectName, string recordId, object record) => Force.UpdateAsync(objectName, recordId, record);
        public Task<SuccessResponse> UpsertExternalAsync(string objectName, string externalFieldName, string externalId, object record) => Force.UpsertExternalAsync(objectName, externalFieldName, Uri.EscapeDataString(externalId), record);
        public Task<SuccessResponse> UpsertExternalAsync(string objectName, string externalFieldName, string externalId, object record, bool ignoreNull) => Force.UpsertExternalAsync(objectName, externalFieldName, Uri.EscapeDataString(externalId), record, ignoreNull);
        public Task<bool> DeleteAsync(string objectName, string recordId) => Force.DeleteAsync(objectName, recordId);
        public Task<bool> DeleteExternalAsync(string objectName, string externalFieldName, string externalId) => Force.DeleteExternalAsync(objectName, externalFieldName, Uri.EscapeDataString(externalId));
        public Task<DescribeGlobalResult<T>> GetObjectsAsync<T>() => Force.GetObjectsAsync<T>();
        public Task<T> BasicInformationAsync<T>(string objectName) => Force.BasicInformationAsync<T>(objectName);
        public Task<T> DescribeAsync<T>(string objectName) => Force.DescribeAsync<T>(objectName);
        public Task<T> GetDeleted<T>(string objectName, DateTime startDateTime, DateTime endDateTime) => Force.GetDeleted<T>(objectName, startDateTime, endDateTime);
        public Task<T> GetUpdated<T>(string objectName, DateTime startDateTime, DateTime endDateTime) => Force.GetUpdated<T>(objectName, startDateTime, endDateTime);
        public Task<T> DescribeLayoutAsync<T>(string objectName) => Force.DescribeLayoutAsync<T>(objectName);
        public Task<T> DescribeLayoutAsync<T>(string objectName, string recordTypeId) => Force.DescribeLayoutAsync<T>(objectName, recordTypeId);
        public Task<T> RecentAsync<T>(int limit = 200) => Force.RecentAsync<T>(limit);
        public Task<List<T>> SearchAsync<T>(string query) => Force.SearchAsync<T>(query);
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

        public async Task<T> LimitsAsync<T>()
        {
            return await JsonHttp.HttpGetAsync<T>("limits").ConfigureAwait(false);
        }

        #region sobjects

        public async Task<T> NamedLayoutsAync<T>(string objectName, string layoutName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(layoutName)) throw new ArgumentNullException("layoutName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/describe/namedLayouts/{layoutName}").ConfigureAwait(false);
        }

        public async Task<T> ApprovalLayoutsAync<T>(string objectName, string approvalProcessName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");


            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/describe/approvalLayouts/{approvalProcessName}").ConfigureAwait(false);
        }

        public async Task<T> CompactLayoutsAync<T>(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/describe/compactLayouts/").ConfigureAwait(false);
        }

        public async Task<T> DescribeLayoutsAync<T>(string objectName = "Global")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/describe/layouts/").ConfigureAwait(false);
        }

        public async Task<T> PlatformActionAync<T>()
        {
            return await JsonHttp.HttpGetAsync<T>($"sobjects/PlatformAction").ConfigureAwait(false);
        }

        public async Task<T> QuickActionsAync<T>(string objectName, string actionName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/quickActions/{actionName}").ConfigureAwait(false);
        }

        public async Task<T> QuickActionsDetailsAync<T>(string objectName, string actionName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException("actionName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/quickActions/{actionName}/describe/").ConfigureAwait(false);
        }

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

        public async Task<T> RetrieveExternalAsync<T>(string objectName, string externalFieldName, string externalId, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException("externalId");
            if (fields == null) throw new ArgumentNullException("fields");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}" +
                (fields?.Length > 0 ? $"?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}" : "")).ConfigureAwait(false);
        }

        public async Task<System.IO.Stream> RichTextImageRetrieveAsync(string objectName, string recordId, string fieldName, string contentReferenceId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException("fieldName");
            if (string.IsNullOrEmpty(contentReferenceId)) throw new ArgumentNullException("contentReferenceId");

            return await JsonHttp.HttpGetBlobAsync($"sobjects/{objectName}/{recordId}/richTextImageFields/{fieldName}/{contentReferenceId}").ConfigureAwait(false);
        }

        public async Task<T> RelationshipsAync<T>(string objectName, string recordId, string relationshipFieldName, string[] fields = null)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (string.IsNullOrEmpty(relationshipFieldName)) throw new ArgumentNullException("relationshipFieldName");

            return await JsonHttp.HttpGetAsync<T>($"sobjects/{objectName}/{recordId}/{relationshipFieldName}" +
                (fields?.Length > 0 ? $"?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}" : "")).ConfigureAwait(false);
        }

        #endregion

        public async Task<UserInfo> UserInfoAsync()
        {
            return await Force.UserInfo<UserInfo>(Id).ConfigureAwait(false);
        }

        public async Task<List<T>> VersionsAsync<T>()
        {
            return await JsonHttp.HttpGetAsync<List<T>>(new Uri(new Uri(InstanceUrl), "/services/data")).ConfigureAwait(false);
        }

        public Task<T> ResourcesAsync<T>()
        {
            return ResourcesAsync<T>(ApiVersion);
        }

        public async Task<T> ResourcesAsync<T>(string apiVersion)
        {
            if (string.IsNullOrEmpty(apiVersion)) throw new ArgumentNullException("apiVersion");

            return await JsonHttp.HttpGetAsync<T>(new Uri(new Uri(InstanceUrl), $"/services/data/{apiVersion}")).ConfigureAwait(false);
        }

        public IEnumerable<JObject> ToEnumerable(JObject parent, string field)
        {
            if (parent == null) throw new ArgumentNullException("parent");

            if (parent[field] == null || parent[field].Type == JTokenType.Null)
            {
                return Enumerable.Empty<JObject>();
            }

            return ToEnumerable<JObject>(parent?[field].ToObject<QueryResult<JObject>>());
        }


        public IEnumerable<T> ToEnumerable<T>(QueryResult<T> queryResult)
        {
            return ToEnumerable<T>(queryResult, true);
        }

        public IEnumerable<T> ToEnumerable<T>(QueryResult<T> queryResult, bool runInParallel)
        {
            if (queryResult == null)
            {
                return Enumerable.Empty<T>();
            }

            return Observable.Create<T>(subscribe => Task.Run(async () =>
            {
                try
                {
                    foreach (var row in queryResult.Records)
                    {
                        subscribe.OnNext(row);
                    }

                    if (queryResult.Records.Count < queryResult.TotalSize)
                    {
                        var batchSize = GetBatchSize(queryResult.NextRecordsUrl);

                        if (!runInParallel || batchSize <= 0 || queryResult.Records.Count + batchSize >= queryResult.TotalSize)
                        {
                            await QueryContinuationAsync(queryResult, subscribe).ConfigureAwait(false);
                        }
                        else
                        {
                            await QueryContinuationAsync(queryResult, subscribe, batchSize).ConfigureAwait(false);
                        }
                    }

                    subscribe.OnCompleted();
                }
                catch (Exception ex)
                {
                    subscribe.OnError(ex);
                }
            })).ToEnumerable();

            int GetBatchSize(string url)
            {
                return !int.TryParse(Regex.Match(url ?? "", @"^/services/data/v\d+\.\d+/.+-(\d+)$").Groups[1].Value, out int intVal) ? 0 : intVal < 0 ? 0 : intVal > 2000 ? 2000 : intVal;
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

        private async Task QueryContinuationAsync<T>(QueryResult<T> queryResult, IObserver<T> subscribe, int batchSize)
        {
            // batch size can be changed via Sforce-Query-Options header
            // i.e. request.Query(referenceId, query).HttpHeaders = new HttpHeaders().QueryOptions(200);
            // https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/headers_queryoptions.htm
            // The default is 2,000; the minimum is 200, and the maximum is 2,000.
            // There is no guarantee that the requested batch size is the actual batch size.
            // Changes are made as necessary to maximize performance.
            var batchs = GetQueryBatchs(queryResult.Records.Count, queryResult.TotalSize, batchSize);
            var batchChunks = batchs.Chunk(DNF.COMPOSITE_QUERY_LIMIT);

            var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
            var tasks = new List<Task>();

            foreach (var (batch, batchNo) in batchChunks.Select((batch, batchNo) => (batch, batchNo)))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await throttler.WaitAsync().ConfigureAwait(true);
                    try
                    {
                        foreach (var (record, j) in QueryByChunk<T>(queryResult.NextRecordsUrl, batch).Select((record, j) => (record, j)))
                        {
                            if (batchNo > 0 && j == 0)
                            {
                                await tasks[batchNo - 1].ConfigureAwait(false);
                            }
                            queryResult.Records.Add(record);
                            queryResult.Done = queryResult.Records.Count >= queryResult.TotalSize;
                            subscribe.OnNext(record);
                        }
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            queryResult.NextRecordsUrl = null;
        }

        private IEnumerable<T> QueryByChunk<T>(string nextRecordsUrl, List<(int offset, int size)> batchChunk)
        {
            return Observable.Create<T>(subscribe => Task.Run(async () =>
            {
                try
                {
                    var request = new CompositeRequest();

                    foreach (var (offset, size) in batchChunk)
                    {
                        var url = GetNextUrl(nextRecordsUrl, offset);
                        request.CompositeRequests.Add(new CompositeSubrequest
                        {
                            ReferenceId = $"{offset}",
                            ResponseType = "query",
                            Method = "GET",
                            Url = url
                        });
                    }
                    if (Logger != null) request.CompositeRequests.ForEach(r => Logger.Invoke($"Query Start\t{r.Url}"));
                    var result = await Composite.PostAsync(request).ConfigureAwait(false);
                    if (Logger != null) request.CompositeRequests.ForEach(r => Logger.Invoke($"Query End\t{r.Url}"));

                    foreach (var (offset, size) in batchChunk)
                    {
                        var referenceId = $"{offset}";
                        var query = result.Queries(referenceId);

                        if (query == null || query.Records?.Any() != true)
                        {
                            throw new ForceException(Error.Unknown, result.Errors(referenceId)?.ToString() ?? "ToEnumerable Failed.");
                        }

                        // actual batch size can be more than requested batch size
                        var records = query.Records.Take(size).Cast<T>().ToList();

                        foreach (var record in records)
                        {
                            subscribe.OnNext(record);
                        }

                        // actual batch size can be less than requested batch size
                        if (records.Count < size)
                        {
                            var initialSize = size + records.Count;
                            var total = size - records.Count;
                            var subBatchSize = Math.Min(total, Math.Max(200, records.Count));
                            var subBatchs = GetQueryBatchs(offset + records.Count, offset + size, subBatchSize);
                            foreach (var record in QueryByChunk<T>(nextRecordsUrl, subBatchs))
                            {
                                subscribe.OnNext(record);
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

            string GetNextUrl(string url, int batchNo)
            {
                return Regex.Replace(url ?? "", @"^/services/data/v\d+\.\d+/(.+)-\d+$", $"$1-{batchNo}");
            }


        }

        private List<(int offset, int size)> GetQueryBatchs(int loaded, int total, int batchSize)
        {
            var noOfBatch = (int)Math.Ceiling((total - loaded) / (double)batchSize);
            var batchs = Enumerable.Range(0, noOfBatch).Select(i =>
            {
                var offset = loaded + i * batchSize;
                var last = Math.Min(total, offset + batchSize);
                var size = last - offset;
                return (offset, size);
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
