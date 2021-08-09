using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DotNetForce
{
    public class CompositeClient : ICompositeClient
    {
        public CompositeClient(JsonHttpClient jsonHttp, string apiVersion, Action<string>? logger = null)
        {
            JsonHttp = jsonHttp;
            ApiVersion = apiVersion;
            Logger = logger;
        }

        public JsonHttpClient JsonHttp { get; set; }
        public string ApiVersion { get; set; }
        public Action<string>? Logger { get; set; }

        public async Task<CompositeResult> PostAsync(ICompositeRequest request)
        {
            if (request == null || request.CompositeRequests.Count <= 0) throw new ArgumentNullException(nameof(request));

            try
            {
                var resourceName = $"{request.Prefix}composite";

                if (request.AllOrNone)
                {
                    var requests = request.CompositeRequests;

                    if (requests.Count > Dnf.CompositeLimit) throw new ArgumentOutOfRangeException(nameof(request));
                    if (requests.Count(c => IsQuery(c.ResponseType)) > Dnf.CompositeQueryLimit) throw new ArgumentOutOfRangeException(nameof(request));

                    var inputObject = new JObject
                    {
                        ["allOrNone"] = true,
                        ["compositeRequest"] = JToken.FromObject(requests.Select(req => Dnf.Assign(JObject.FromObject(req), new JObject
                        {
                            ["url"] = DecodeReference($"/services/data/{ApiVersion}/{request.Prefix}{req.Url?.TrimStart('/')}")
                        })))
                    };

                    var result = await JsonHttp.HttpPostAsync<CompositeResultBody>(inputObject, resourceName)
                        .ConfigureAwait(false);
                    var results = new CompositeResult(request.CompositeRequests, result?.CompositeResponse ?? new List<CompositeSubRequestResult>());
                    return results;
                }
                else
                {
                    var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                    var results = new CompositeResult();

                    var chunks = new List<IList<CompositeSubRequest>>();
                    IList<CompositeSubRequest>? chunk = null;

                    foreach (var req in request.CompositeRequests)
                    {
                        var added = false;

                        if (IsQuery(req.ResponseType))
                        {
                            if (chunk != null && chunk.Count(c => IsQuery(c.ResponseType)) < Dnf.CompositeQueryLimit)
                            {
                                chunk.Add(req);
                                added = true;
                            }
                        }
                        else if (chunk?.Count < Dnf.CompositeLimit)
                        {
                            chunk.Add(req);
                            added = true;
                        }

                        if (added) continue;
                        chunk = new List<CompositeSubRequest> { req };
                        chunks.Add(chunk);
                    }

                    var tasks = new List<Task>();

                    foreach (var requests in chunks)
                    {
                        await throttler.WaitAsync()
                            .ConfigureAwait(false);
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var inputObject = new JObject
                                {
                                    ["compositeRequest"] = JToken.FromObject(requests.Select(req => Dnf.Assign(JObject.FromObject(req), new JObject
                                    {
                                        ["url"] = DecodeReference($"/services/data/{ApiVersion}/{request.Prefix}{req.Url?.TrimStart('/')}")
                                    })))
                                };

                                var result = await JsonHttp.HttpPostAsync<CompositeResultBody>(inputObject, resourceName)
                                    .ConfigureAwait(false);
                                results.Add(requests, result?.CompositeResponse ?? new List<CompositeSubRequestResult>());
                            }
                            catch (Exception ex)
                            {
                                var body = new JArray { ex.Message };
                                var responses = requests.Select(req => new CompositeSubRequestResult
                                {
                                    Body = body,
                                    ReferenceId = req.ReferenceId,
                                    HttpStatusCode = 500
                                }).ToList();
                                results.Add(requests, responses);
                            }
                            finally
                            {
                                throttler.Release();
                            }
                        }));
                    }
                    await Task.WhenAll(tasks)
                        .ConfigureAwait(false);
                    return results;
                }
            }
            catch (Exception ex)
            {
                var body = new JArray { ex.Message };
                var responses = request.CompositeRequests.Select(req => new CompositeSubRequestResult
                {
                    Body = body,
                    ReferenceId = req.ReferenceId,
                    HttpStatusCode = 500
                }).ToList();
                var results = new CompositeResult(request.CompositeRequests, responses);
                return results;
            }

            bool IsQuery(string responseType)
            {
                return responseType == "query" || responseType  == "collections";
            }
        }

        public async Task<BatchResult> BatchAsync(IBatchRequest request)
        {
            if (request == null || request.BatchRequests.Count <= 0) throw new ArgumentNullException(nameof(request));

            try
            {
                var resourceName = $"{request.Prefix}composite/batch";

                if (request.HaltOnError)
                {
                    if (request.BatchRequests.Count > Dnf.BatchLimit) throw new ArgumentOutOfRangeException(nameof(request));

                    var inputObject = new JObject
                    {
                        ["batchRequests"] = JToken.FromObject(request.BatchRequests.Select(req => Dnf.Assign(JObject.FromObject(req), new JObject
                        {
                            ["url"] = DecodeReference($"/services/data/{ApiVersion}/{request.Prefix}{req.Url?.TrimStart('/')}")
                        }))),
                        ["haltOnError"] = true
                    };

                    var result = await JsonHttp.HttpPostAsync<BatchResultBody>(inputObject, resourceName)
                        .ConfigureAwait(false);
                    var results = new BatchResult(request.BatchRequests, result?.Results ?? new List<BatchSubRequestResult>());
                    return results;
                }
                else
                {
                    var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                    var results = new BatchResult();

                    var chunks = new List<IList<BatchSubRequest>>();
                    IList<BatchSubRequest>? chunk = null;

                    foreach (var req in request.BatchRequests)
                    {
                        var added = false;

                        if (chunk?.Count < Dnf.BatchLimit)
                        {
                            chunk.Add(req);
                            added = true;
                        }

                        if (added) continue;
                        chunk = new List<BatchSubRequest> { req };
                        chunks.Add(chunk);
                    }

                    var tasks = new List<Task>();

                    foreach (var requests in chunks)
                    {
                        await throttler.WaitAsync()
                            .ConfigureAwait(false);
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var inputObject = new JObject
                                {
                                    ["batchRequests"] = JToken.FromObject(requests.Select(req => Dnf.Assign(JObject.FromObject(req), new JObject
                                    {
                                        ["url"] = DecodeReference($"/services/data/{ApiVersion}/{request.Prefix}{req.Url?.TrimStart('/')}")
                                    })))
                                };


                                var result = await JsonHttp.HttpPostAsync<BatchResultBody>(inputObject, resourceName)
                                    .ConfigureAwait(false);
                                results.Add(requests, result?.Results ?? new List<BatchSubRequestResult>());
                            }
                            catch (Exception ex)
                            {
                                var body = new JArray { ex.Message };
                                var responses = requests.Select(req => new BatchSubRequestResult
                                {
                                    Result = body,
                                    StatusCode = 500
                                }).ToList();
                                results.Add(requests, responses);
                            }
                            finally
                            {
                                throttler.Release();
                            }
                        }));
                    }
                    await Task.WhenAll(tasks)
                        .ConfigureAwait(false);
                    return results;
                }
            }
            catch (Exception ex)
            {
                var body = new JArray { ex.Message };
                var responses = request.BatchRequests.Select(req => new BatchSubRequestResult
                {
                    Result = body,
                    StatusCode = 500
                }).ToList();
                var results = new BatchResult(request.BatchRequests, responses);
                return results;
            }
        }

        public async Task<SaveResponse> CreateTreeAsync<T>(string objectName, IList<T> objectTree)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            if (objectTree == null || !objectTree.Any()) throw new ArgumentNullException(nameof(objectTree));

            if (typeof(IAttributedObject).IsAssignableFrom(typeof(T)))
                return await Dnf.TryDeserializeObjectAsync(
                    JsonHttp.HttpPostAsync<SaveResponse>(
                        new CreateRequest { Records = objectTree.Cast<IAttributedObject>().ToList() },
                        $"composite/tree/{objectName}"))
                    .ConfigureAwait(false) ?? new SaveResponse();
            return await Dnf.TryDeserializeObjectAsync(
                JsonHttp.HttpPostAsync<SaveResponse>(
                    new JObject { ["records"] = JToken.FromObject(objectTree) },
                    $"composite/tree/{objectName}"))
                .ConfigureAwait(false) ?? new SaveResponse();
        }

        protected static string DecodeReference(string? value)
        {
            var pattern = Uri.EscapeDataString("@{") +
                          $"[0-9a-z](?:[_.0-9a-z]|{Uri.EscapeDataString("[")}|{Uri.EscapeDataString("]")})*" + //$".+" +
                          Uri.EscapeDataString("}");

            string Evaluator(Match m)
            {
                return Uri.UnescapeDataString(m.Value);
            }

            var decoded = Regex.Replace(value ?? "", pattern, Evaluator, RegexOptions.IgnoreCase);
            return decoded;
        }

        public IAsyncEnumerable<QueryResult<JObject>> QueryAsync(string query)
        {
            return QueryAsync<JObject>(query);
        }

        public async IAsyncEnumerable<QueryResult<T>> QueryAsync<T>(string query)
        {
            var request = new CompositeRequest();
            request.Query("q", query);
            var compositeResult = await PostAsync(request)
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
            var compositeResult = await PostAsync(request)
                .ConfigureAwait(false);
            compositeResult.Assert();
            var result = compositeResult.Queries<T>("q");
            await foreach (var nextResult in QueryByLocatorAsync(result)
                .ConfigureAwait(false))
                yield return nextResult;
        }

        public IAsyncEnumerable<QueryResult<T>> QueryByLocatorAsync<T>(QueryResult<T>? queryResult) => QueryByLocatorAsync(queryResult, Dnf.CompositeLimit);
        public async IAsyncEnumerable<QueryResult<T>> QueryByLocatorAsync<T>(QueryResult<T>? queryResult, int compositeLimit)
        {
            if (queryResult == null) yield break;
            yield return queryResult;
            // batch size can be changed via Sforce-Query-Options header
            // i.e. request.Query(referenceId, query).HttpHeaders = new HttpHeaders().QueryOptions(200);
            // https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/headers_queryoptions.htm
            // The default is 2,000; the minimum is 200, and the maximum is 2,000.
            // There is no guarantee that the requested batch size is the actual batch size.
            // Changes are made as necessary to maximize performance.
            if (compositeLimit > Dnf.CompositeQueryLimit)
                compositeLimit = Dnf.CompositeQueryLimit;
            var batchSize = GetBatchSizeByUrl(queryResult.NextRecordsUrl);
            var batches = PredictQueryLocators(queryResult.NextRecordsUrl, queryResult.Records?.Count ?? 0,
                queryResult.TotalSize - (queryResult.Records?.Count ?? 0), batchSize);
            var chunks = EnumerableChunk.Create(batches, compositeLimit);

            foreach (var (batch, _) in chunks.Select((batch, batchIdx) => (batch, batchIdx)))
            {
                //await Dnf.QueryCursorThrottler.WaitAsync().ConfigureAwait(true);
                //try
                //{
                await foreach (var chunkResult in QueryByChunkAsync<T>(batch)
                    .ConfigureAwait(false))
                    // if (batchIdx > 0 && recordIdx == 0)
                    //     await tasks[batchIdx - 1].ConfigureAwait(false);
                    //queryResult.Records.Add(record);
                    //queryResult.Done = queryResult.Records.Count >= queryResult.TotalSize;
                    yield return chunkResult;
                //}
                //finally
                //{
                //    Dnf.QueryCursorThrottler.Release();
                //}
            }
        }

        private async IAsyncEnumerable<QueryResult<T>> QueryByChunkAsync<T>(
            IList<(int offset, int size, string? queryLocator, string referenceId)> chunk)
        {
            var request = new CompositeRequest();

            foreach (var (_, _, queryLocator, referenceId) in chunk)
                request.CompositeRequests.Add(new CompositeSubRequest
                {
                    ReferenceId = referenceId,
                    ResponseType = "query",
                    Method = "GET",
                    Url = queryLocator
                });

            Logger?.Invoke(string.Join(Environment.NewLine, request.CompositeRequests.Select(r => r.Url)));
            Logger?.Invoke($@"Query Start {JsonConvert.SerializeObject(request.CompositeRequests)}");

            var result = await PostAsync(request)
                .ConfigureAwait(false);

            result.Assert();

            Logger?.Invoke($@"Query End {JsonConvert.SerializeObject(result.Queries().Select(que => new
            {
                ReferenceId = que.Key ?? string.Empty,
                que.Value?.Done,
                que.Value?.TotalSize,
                que.Value?.NextRecordsUrl,
                RecordsCount = que.Value?.Records?.Count ?? 0
            }))}");

            foreach (var (offset, size, nextUrl, referenceId) in chunk)
            {
                var query = result.Queries<T>(referenceId);

                if (query.Records == null || query.Records.Any() != true)
                    throw new ForceException(Error.Unknown, result.Errors(referenceId)?.ToString()
                        ?? "GetEnumerable Failed.");

                // actual batch size can be more than requested batch size
                if (query.Records.Count > size) query.Records = query.Records.Take(size).ToList();
                yield return query;

                // actual batch size can be less than requested batch size
                if (query.Records.Count >= size) continue;
                var remaining = size - query.Records.Count;
                var subBatchSize = Math.Min(remaining, Math.Max(200, query.Records.Count));
                var subBatches = PredictQueryLocators(nextUrl, offset + query.Records.Count, remaining, subBatchSize);
                await foreach (var batchResult in QueryByChunkAsync<T>(subBatches)
                    .ConfigureAwait(false))
                    yield return batchResult;
            }
        }

        private static int GetBatchSizeByUrl(string? url)
        {
            if (url == null) return 2000;
            url = Regex.Replace(url, @"^/services/data/[^/]+/", "");
            return !int.TryParse(Regex.Match(url, @"^(?:.*query/01g[^/]+)-(\d+)$").Groups[1].Value, out var intVal)
                ? 2000
                : intVal;
        }

        private static string? GetQueryLocatorByBatchNo(string? url, int batchNo)
        {
            if (url == null) return null;
            url = Regex.Replace(url, @"^/services/data/[^/]+/", "");
            var nextUrl = Regex.Replace(url, @"^(.*query/[^/]+)-\d+$", $"$1-{batchNo}");
            return nextUrl;
        }

        private static IList<(int offset, int size, string? queryLocator, string referenceId)> PredictQueryLocators(string? queryLocator, int loaded, int remaining, int batchSize,
            string referenceFormat = "{0}_{1}")
        {
            var noOfBatch = (int)Math.Ceiling(remaining / (double)batchSize);
            List<(int offset, int size, string? queryLocator, string referenceId)> batches = Enumerable.Range(0, noOfBatch).Select(i =>
            {
                var offset = loaded + i * batchSize;
                var last = Math.Min(loaded + remaining, offset + batchSize);
                var size = last - offset;
                var nextQueryLocator = GetQueryLocatorByBatchNo(queryLocator, offset);
                var referenceId = string.Format(referenceFormat, offset, size);
                return (offset, size, queryLocator: nextQueryLocator, referenceId);
            }).ToList();
            return batches;
        }

        public IAsyncEnumerable<QueryResult<JObject>> QueryByIdsAsync(IEnumerable<string> source, string templateSoql, string template) =>
            QueryByIdsAsync<JObject>(source, templateSoql, template);
        public async IAsyncEnumerable<QueryResult<T>> QueryByIdsAsync<T>(
            IEnumerable<string> source, string templateSoql, string template)
        {
            var soqlList = Dnf.ChunkIds(source, templateSoql, template);
            foreach (var soql in soqlList)
            await foreach (var result in QueryAsync<T>(soql)
                .ConfigureAwait(false))
            await foreach (var nextResult in QueryByLocatorAsync(result)
                .ConfigureAwait(false))
                yield return nextResult;
        }

        public IAsyncEnumerable<QueryResult<JObject>> QueryByFieldValuesAsync(IEnumerable<string> source, string templateSoql, string template) =>
            QueryByFieldValuesAsync<JObject>(source, templateSoql, template);
        public async IAsyncEnumerable<QueryResult<T>> QueryByFieldValuesAsync<T>(IEnumerable<string> source, string templateSoql, string template)
        {
            var soqlList = Dnf.ChunkSoqlByFieldValues(source, templateSoql, template);
            foreach (var soql in soqlList)
            await foreach (var result in QueryAsync<T>(soql)
                .ConfigureAwait(false))
            await foreach (var nextResult in QueryByLocatorAsync(result)
                .ConfigureAwait(false))
                yield return nextResult;
        }

        #region Collections

        public Task<CompositeResult> CreateAsync<T>(IList<T> records)
        {
            return CreateAsync(records, false);
        }

        public async Task<CompositeResult> CreateAsync<T>(IList<T> records, bool allOrNone)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));

            if (allOrNone)
            {
                if (records.Count > 200 * Dnf.CompositeQueryLimit) throw new ArgumentOutOfRangeException(nameof(records));

                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                    request.Create($"{chunkIdx}", true, chunk);
                Logger?.Invoke("Create Start");
                var results = await PostAsync(request)
                    .ConfigureAwait(false);
                Logger?.Invoke("Create End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in EnumerableChunk.Create(records, 200 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync()
                        .ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Create($"{batchNo}_{chunkIdx}", false, chunk);
                            Logger?.Invoke($"Create Start {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");
                            var result = await PostAsync(request)
                                .ConfigureAwait(false);
                            Logger?.Invoke($"Create End {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");

                            if (batchNo > 0) await tasks[batchNo - 1]
                                .ConfigureAwait(false);

                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks)
                    .ConfigureAwait(false);
                return results;
            }
        }

        public async Task<CompositeResult> RetrieveAsync(string objectName, IList<string> ids, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            if (fields == null || fields.Length == 0) throw new ArgumentNullException(nameof(fields));

            var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
            var tasks = new List<Task>();
            var results = new CompositeResult();

            foreach (var (batch, batchNo) in EnumerableChunk.Create(ids, 2000 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
            {
                await throttler.WaitAsync()
                    .ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var request = new CompositeRequest();
                        foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(batch, 2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            request.Retrieve($"{batchNo}_{chunkIdx}", objectName, chunk, fields);
                        Logger?.Invoke($"Retrieve Start {batchNo * 2000 * Dnf.CompositeQueryLimit + batch.Count}");
                        var result = await PostAsync(request)
                            .ConfigureAwait(false);
                        Logger?.Invoke($"Retrieve End {batchNo * 2000 * Dnf.CompositeQueryLimit + batch.Count}");

                        if (batchNo > 0) await tasks[batchNo - 1]
                            .ConfigureAwait(false);

                        results.Add(result);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);
            return results;
        }

        public async Task<CompositeResult> RetrieveExternalAsync(string objectName, string externalFieldName, IList<string> externalIds, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (externalIds == null || !externalIds.Any()) throw new ArgumentNullException(nameof(externalIds));
            if (fields == null || fields.Length == 0) throw new ArgumentNullException(nameof(fields));

            var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
            var tasks = new List<Task>();
            var results = new CompositeResult();

            foreach (var (batch, batchNo) in EnumerableChunk.Create(externalIds, 2000 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
            {
                await throttler.WaitAsync()
                    .ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var request = new CompositeRequest();
                        foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(batch, 2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            request.RetrieveExternal($"{batchNo}_{chunkIdx}", objectName, externalFieldName, chunk, fields);
                        Logger?.Invoke($"Retrieve Start {batchNo * 2000 * Dnf.CompositeQueryLimit + batch.Count}");
                        var result = await PostAsync(request)
                            .ConfigureAwait(false);
                        Logger?.Invoke($"Retrieve End {batchNo * 2000 * Dnf.CompositeQueryLimit + batch.Count}");

                        if (batchNo > 0) await tasks[batchNo - 1]
                            .ConfigureAwait(false);

                        results.Add(result);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);
            return results;
        }

        public Task<CompositeResult> UpdateAsync<T>(IList<T> records)
        {
            return UpdateAsync(records, false);
        }

        public async Task<CompositeResult> UpdateAsync<T>(IList<T> records, bool allOrNone)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));

            if (allOrNone)
            {
                if (records.Count > 200 * Dnf.CompositeQueryLimit) throw new ArgumentOutOfRangeException(nameof(records));

                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Update($"{chunkIdx}", true, chunk);
                Logger?.Invoke("Update Start");
                var results = await PostAsync(request)
                    .ConfigureAwait(false);
                Logger?.Invoke("Update End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in EnumerableChunk.Create(records, 200 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync()
                        .ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Update($"{batchNo}_{chunkIdx}", false, chunk);
                            Logger?.Invoke($"Update Start {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");
                            var result = await PostAsync(request)
                                .ConfigureAwait(false);
                            Logger?.Invoke($"Update End {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");

                            if (batchNo > 0) await tasks[batchNo - 1]
                                .ConfigureAwait(false);

                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks)
                    .ConfigureAwait(false);
                return results;
            }
        }

        //public Task<CompositeResult> UpsertExternalAsync<T>(string externalFieldName, IList<T> records)
        //{
        //    return UpsertExternalAsync(externalFieldName, records, false);
        //}

        //public async Task<CompositeResult> UpsertExternalAsync<T>(string externalFieldName, IList<T> records, bool allOrNone)
        //{
        //    if (records == null) throw new ArgumentNullException("records");

        //    if (allOrNone)
        //    {
        //        if (records.Count() > 200 * Dnf.COMPOSITE_QUERY_LIMIT) throw new ArgumentOutOfRangeException("records");

        //        var request = new CompositeRequest();
        //        foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
        //        {
        //            request.Update($"{chunkIdx}", allOrNone, chunk);
        //        }
        //        Logger?.Invoke($"Update Start");
        //        var results = await PostAsync(request).ConfigureAwait(false);
        //        Logger?.Invoke($"Update End");
        //        return results;
        //    }
        //    else
        //    {
        //        var throttler = new SemaphoreSlim(Dnf.ConcurrentRequestLimit, Dnf.ConcurrentRequestLimit);
        //        var tasks = new List<Task>();
        //        var results = new CompositeResult();

        //        foreach (var (batch, batchNo) in EnumerableChunk.Create(records, 200 * Dnf.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
        //        {
        //            await throttler.WaitAsync().ConfigureAwait(false);
        //            tasks.Add(Task.Run(async () =>
        //            {
        //                try
        //                {
        //                    var request = new CompositeRequest();
        //                    foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
        //                    {
        //                        request.UpsertExternal($"{batchNo}_{chunkIdx}", allOrNone, externalFieldName, chunk);
        //                    }
        //                    Logger?.Invoke($"Update Start {batchNo * 200 * Dnf.COMPOSITE_QUERY_LIMIT + batch.Count}");
        //                    var result = await PostAsync(request).ConfigureAwait(false);
        //                    Logger?.Invoke($"Update End {batchNo * 200 * Dnf.COMPOSITE_QUERY_LIMIT + batch.Count}");

        //                    if (batchNo > 0)
        //                    {
        //                        await tasks[batchNo - 1].ConfigureAwait(false);
        //                    }

        //                    results.Add(result);
        //                }
        //                finally
        //                {
        //                    throttler.Release();
        //                }
        //            }));
        //        }

        //        await Task.WhenAll(tasks).ConfigureAwait(false);
        //        return results;
        //    }
        //}

        public Task<CompositeResult> DeleteAsync(IList<string> ids)
        {
            return DeleteAsync(ids, false);
        }

        public async Task<CompositeResult> DeleteAsync(IList<string> ids, bool allOrNone)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));

            if (allOrNone)
            {
                if (ids.Count > 200 * Dnf.CompositeQueryLimit) throw new ArgumentOutOfRangeException(nameof(ids));

                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(ids, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Delete($"{chunkIdx}", true, chunk.ToArray());
                Logger?.Invoke("Delete Start");
                var results = await PostAsync(request)
                    .ConfigureAwait(false);
                Logger?.Invoke("Delete End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in EnumerableChunk.Create(ids, 200 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync()
                        .ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                                request.Delete($"{batchNo}_{chunkIdx}", false, chunk.ToArray());
                            Logger?.Invoke($"Delete Start {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");
                            var result = await PostAsync(request)
                                .ConfigureAwait(false);
                            Logger?.Invoke($"Delete End {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");
                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks)
                    .ConfigureAwait(false);
                return results;
            }
        }

        #endregion
    }
}
