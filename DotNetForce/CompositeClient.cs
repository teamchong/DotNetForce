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

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
    public class CompositeClient : ICompositeClient
    {
        public CompositeClient(JsonHttpClient jsonHttp, string apiVersion)
            : this(jsonHttp, apiVersion, null) { }

        public CompositeClient(JsonHttpClient jsonHttp, string apiVersion, Action<string> logger)
        {
            JsonHttp = jsonHttp;
            ApiVersion = apiVersion;
            Logger = logger;
        }

        public JsonHttpClient JsonHttp { get; set; }
        public string ApiVersion { get; set; }
        public Action<string> Logger { get; set; }

        public async Task<CompositeResult> PostAsync(ICompositeRequest request)
        {
            if (request == null || request.CompositeRequests.Count <= 0) throw new ArgumentNullException(nameof(request));

            try
            {
                var urlSuffix = $"{request.Prefix}composite";

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
                            ["url"] = DecodeReference($"/services/data/{ApiVersion}/{request.Prefix}{req.Url.TrimStart('/')}")
                        })))
                    };

                    var result = await JsonHttp.HttpPostAsync<CompositeResultBody>(inputObject, urlSuffix).ConfigureAwait(false);
                    var results = new CompositeResult(request.CompositeRequests, result.CompositeResponse);
                    return results;
                }
                else
                {
                    var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                    var results = new CompositeResult();

                    var chunks = new List<IList<CompositeSubRequest>>();
                    IList<CompositeSubRequest> chunk = null;

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
                        else if (chunk != null && chunk.Count < Dnf.CompositeLimit)
                        {
                            chunk.Add(req);
                            added = true;
                        }

                        if (!added)
                        {
                            chunk = new List<CompositeSubRequest> { req };
                            chunks.Add(chunk);
                        }
                    }

                    var tasks = new List<Task>();

                    foreach (var requests in chunks)
                    {
                        await throttler.WaitAsync().ConfigureAwait(false);
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var inputObject = new JObject
                                {
                                    ["compositeRequest"] = JToken.FromObject(requests.Select(req => Dnf.Assign(JObject.FromObject(req), new JObject
                                    {
                                        ["url"] = DecodeReference($"/services/data/{ApiVersion}/{request.Prefix}{req.Url.TrimStart('/')}")
                                    })))
                                };

                                var result = await JsonHttp.HttpPostAsync<CompositeResultBody>(inputObject, urlSuffix).ConfigureAwait(false);
                                results.Add(requests, result.CompositeResponse);
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
                    await Task.WhenAll(tasks).ConfigureAwait(false);
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
                return responseType == "query" || responseType == "collections";
            }
        }

        public async Task<BatchResult> BatchAsync(IBatchRequest request)
        {
            if (request == null || request.BatchRequests.Count <= 0) throw new ArgumentNullException(nameof(request));

            try
            {
                var urlSuffix = $"{request.Prefix}composite/batch";

                if (request.HaltOnError)
                {
                    if (request.BatchRequests.Count > Dnf.BatchLimit) throw new ArgumentOutOfRangeException(nameof(request));

                    var inputObject = new JObject
                    {
                        ["batchRequests"] = JToken.FromObject(request.BatchRequests.Select(req => Dnf.Assign(JObject.FromObject(req), new JObject
                        {
                            ["url"] = DecodeReference($"/services/data/{ApiVersion}/{request.Prefix}{req.Url.TrimStart('/')}")
                        }))),
                        ["haltOnError"] = true
                    };

                    var result = await JsonHttp.HttpPostAsync<BatchResultBody>(inputObject, urlSuffix).ConfigureAwait(false);
                    var results = new BatchResult(request.BatchRequests, result.Results);
                    return results;
                }
                else
                {
                    var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                    var results = new BatchResult();

                    var chunks = new List<IList<BatchSubRequest>>();
                    IList<BatchSubRequest> chunk = null;

                    foreach (var req in request.BatchRequests)
                    {
                        var added = false;

                        if (chunk != null && chunk.Count < Dnf.BatchLimit)
                        {
                            chunk.Add(req);
                            added = true;
                        }

                        if (!added)
                        {
                            chunk = new List<BatchSubRequest> { req };
                            chunks.Add(chunk);
                        }
                    }

                    var tasks = new List<Task>();

                    foreach (var requests in chunks)
                    {
                        await throttler.WaitAsync().ConfigureAwait(false);
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var inputObject = new JObject
                                {
                                    ["batchRequests"] = JToken.FromObject(requests.Select(req => Dnf.Assign(JObject.FromObject(req), new JObject
                                    {
                                        ["url"] = DecodeReference($"/services/data/{ApiVersion}/{request.Prefix}{req.Url.TrimStart('/')}")
                                    })))
                                };


                                var result = await JsonHttp.HttpPostAsync<BatchResultBody>(inputObject, urlSuffix).ConfigureAwait(false);
                                results.Add(requests, result.Results);
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
                    await Task.WhenAll(tasks).ConfigureAwait(false);
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

        private IList<(int offset, int size, string queryLocator, string referenceId)> PredictQueryLocators(string queryLocator, int loaded, int remaining, int batchSize,
            string referenceFormat = "{0}_{1}")
        {
            var noOfBatch = (int)Math.Ceiling(remaining / (double)batchSize);
            List<(int offset, int size, string queryLocator, string referenceId)> batches = Enumerable.Range(0, noOfBatch).Select(i =>
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

        public async IAsyncEnumerable<T> GetAsyncEnumerableByQueryResult<T>(QueryResult<T> queryResult)
        {
            if (queryResult == null) yield break;

            var batchSize = GetBatchSizeByUrl(queryResult.NextRecordsUrl);

            foreach (var row in queryResult.Records)
                //subscribe.OnNext(await IncludeRelationships(row).ConfigureAwait(false));
                yield return row;

            if (queryResult.Records.Count < queryResult.TotalSize)
                await foreach (var row in GetAsyncEnumerableByQueryResult(queryResult, batchSize))
                    //subscribe.OnNext(await IncludeRelationships(row).ConfigureAwait(false));
                    yield return row;
        }

        public async IAsyncEnumerable<T> GetAsyncEnumerableByQueryResult<T>(QueryResult<T> queryResult, int batchSize)
        {
            // batch size can be changed via Sforce-Query-Options header
            // i.e. request.Query(referenceId, query).HttpHeaders = new HttpHeaders().QueryOptions(200);
            // https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/headers_queryoptions.htm
            // The default is 2,000; the minimum is 200, and the maximum is 2,000.
            // There is no guarantee that the requested batch size is the actual batch size.
            // Changes are made as necessary to maximize performance.
            var batches = PredictQueryLocators(queryResult.NextRecordsUrl, queryResult.Records.Count,
                queryResult.TotalSize - queryResult.Records.Count, batchSize);
            var chunks = Dnf.Chunk(batches, Dnf.CompositeQueryLimit);

            foreach (var (batch, batchIdx) in chunks.Select((batch, batchIdx) => (batch, batchIdx)))
            {
                //await Dnf.QueryCursorThrottler.WaitAsync().ConfigureAwait(true);
                //try
                //{
                await foreach (var (record, recordIdx) in GetAsyncEnumerableByChunk<T>(batch))
                {
                    // if (batchIdx > 0 && recordIdx == 0)
                    //     await tasks[batchIdx - 1].ConfigureAwait(false);
                    //queryResult.Records.Add(record);
                    //queryResult.Done = queryResult.Records.Count >= queryResult.TotalSize;
                    yield return record;
                }
                //}
                //finally
                //{
                //    Dnf.QueryCursorThrottler.Release();
                //}
            }
        }

        private async IAsyncEnumerable<(T record, string referenceId)> GetAsyncEnumerableByChunk<T>(
            IList<(int offset, int size, string queryLocator, string referenceId)> chunk)
        {
            var request = new CompositeRequest();

            foreach (var (offset, size, queryLocator, referenceId) in chunk)
                request.CompositeRequests.Add(new CompositeSubRequest
                {
                    ReferenceId = referenceId,
                    ResponseType = "query",
                    Method = "GET",
                    Url = queryLocator
                });

            Logger?.Invoke($@"Query Start {JsonConvert.SerializeObject(request.CompositeRequests)}");

            var result = await PostAsync(request).ConfigureAwait(false);

            Dnf.ThrowIfError(result);

            Logger?.Invoke($@"Query End {JsonConvert.SerializeObject(result.Queries().Select(que => new
            {
                ReferenceId = que.Key,
                que.Value.Done,
                que.Value.TotalSize,
                que.Value.NextRecordsUrl,
                RecordsCount = que.Value.Records.Count
            }))}");

            foreach (var (offset, size, nextUrl, referenceId) in chunk)
            {
                var query = result.Queries(referenceId);

                if (query == null || query.Records?.Any() != true)
                    throw new ForceException(Error.Unknown, result.Errors(referenceId)?.ToString()
                        ?? "GetEnumerable Failed.");

                // actual batch size can be more than requested batch size
                var records = query.Records.Take(size).Cast<T>().ToList();

                foreach (var record in records)
                    yield return (record, referenceId);

                // actual batch size can be less than requested batch size
                if (records.Count < size)
                {
                    var remaining = size - records.Count;
                    var subBatchSize = Math.Min(remaining, Math.Max(200, records.Count));
                    var subBatches = PredictQueryLocators(nextUrl, offset + records.Count, remaining, subBatchSize);
                    await foreach (var batchResult in GetAsyncEnumerableByChunk<T>(subBatches))
                        yield return (batchResult.record, batchResult.referenceId);
                }
            }
        }

        private int GetBatchSizeByUrl(string url)
        {
            if (url == null) return 2000;
            url = Regex.Replace(url, @"^/services/data/[^/]+/", "");
            return !int.TryParse(Regex.Match(url, @"^(?:.*query/01g[^/]+)-(\d+)$").Groups[1].Value, out var intVal)
                ? 2000
                : intVal < 1
                    ? 1
                    : intVal > 2000
                        ? 2000
                        : intVal;
        }

        private string GetQueryLocatorByBatchNo(string url, int batchNo)
        {
            if (url == null) return null;
            url = Regex.Replace(url, @"^/services/data/[^/]+/", "");
            var nextUrl = Regex.Replace(url, @"^(.*query/[^/]+)-\d+$", $"$1-{batchNo}");
            return nextUrl;
        }

        public async Task<SaveResponse> CreateTreeAsync<T>(string objectName, IList<T> objectTree)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            if (objectTree == null || !objectTree.Any()) throw new ArgumentNullException(nameof(objectTree));

            if (typeof(IAttributedObject).IsAssignableFrom(typeof(T)))
                return await Dnf.TryDeserializeObjectAsync(Task.Run(async () =>
                {
                    var result = await JsonHttp.HttpPostAsync<SaveResponse>(
                        new CreateRequest { Records = objectTree.Cast<IAttributedObject>().ToList() },
                        $"composite/tree/{objectName}").ConfigureAwait(false);
                    return result;
                }));
            return await Dnf.TryDeserializeObjectAsync(Task.Run(async () =>
            {
                var result = await JsonHttp.HttpPostAsync<SaveResponse>(
                    new JObject { ["records"] = JToken.FromObject(objectTree) },
                    $"composite/tree/{objectName}").ConfigureAwait(false);
                return result;
            }));
        }

        protected string DecodeReference(string value)
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
                foreach (var (chunk, chunkIdx) in Dnf.Chunk(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Create($"{chunkIdx}", true, chunk);
                Logger?.Invoke("Create Start");
                var results = await PostAsync(request).ConfigureAwait(false);
                Logger?.Invoke("Create End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in Dnf.Chunk(records, 200 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync().ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in Dnf.Chunk(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Create($"{batchNo}_{chunkIdx}", false, chunk);
                            Logger?.Invoke($"Create Start {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Create End {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");

                            if (batchNo > 0) await tasks[batchNo - 1].ConfigureAwait(false);

                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
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

            foreach (var (batch, batchNo) in Dnf.Chunk(ids, 2000 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
            {
                await throttler.WaitAsync().ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var request = new CompositeRequest();
                        foreach (var (chunk, chunkIdx) in Dnf.Chunk(batch, 2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            request.Retrieve($"{batchNo}_{chunkIdx}", objectName, chunk, fields);
                        Logger?.Invoke($"Retrieve Start {batchNo * 2000 * Dnf.CompositeQueryLimit + batch.Count}");
                        var result = await PostAsync(request).ConfigureAwait(false);
                        Logger?.Invoke($"Retrieve End {batchNo * 2000 * Dnf.CompositeQueryLimit + batch.Count}");

                        if (batchNo > 0) await tasks[batchNo - 1].ConfigureAwait(false);

                        results.Add(result);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
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

            foreach (var (batch, batchNo) in Dnf.Chunk(externalIds, 2000 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
            {
                await throttler.WaitAsync().ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var request = new CompositeRequest();
                        foreach (var (chunk, chunkIdx) in Dnf.Chunk(batch, 2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            request.RetrieveExternal($"{batchNo}_{chunkIdx}", objectName, externalFieldName, chunk, fields);
                        Logger?.Invoke($"Retrieve Start {batchNo * 2000 * Dnf.CompositeQueryLimit + batch.Count}");
                        var result = await PostAsync(request).ConfigureAwait(false);
                        Logger?.Invoke($"Retrieve End {batchNo * 2000 * Dnf.CompositeQueryLimit + batch.Count}");

                        if (batchNo > 0) await tasks[batchNo - 1].ConfigureAwait(false);

                        results.Add(result);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
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
                foreach (var (chunk, chunkIdx) in Dnf.Chunk(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Update($"{chunkIdx}", true, chunk);
                Logger?.Invoke("Update Start");
                var results = await PostAsync(request).ConfigureAwait(false);
                Logger?.Invoke("Update End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in Dnf.Chunk(records, 200 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync().ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in Dnf.Chunk(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Update($"{batchNo}_{chunkIdx}", false, chunk);
                            Logger?.Invoke($"Update Start {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Update End {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");

                            if (batchNo > 0) await tasks[batchNo - 1].ConfigureAwait(false);

                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
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
        //        foreach (var (chunk, chunkIdx) in Dnf.Chunk(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
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

        //        foreach (var (batch, batchNo) in Dnf.Chunk(records, 200 * Dnf.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
        //        {
        //            await throttler.WaitAsync().ConfigureAwait(false);
        //            tasks.Add(Task.Run(async () =>
        //            {
        //                try
        //                {
        //                    var request = new CompositeRequest();
        //                    foreach (var (chunk, chunkIdx) in Dnf.Chunk(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
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
                foreach (var (chunk, chunkIdx) in Dnf.Chunk(ids, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx))) request.Delete($"{chunkIdx}", true, chunk.ToArray());
                Logger?.Invoke("Delete Start");
                var results = await PostAsync(request).ConfigureAwait(false);
                Logger?.Invoke("Delete End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(Dnf.DefaultConcurrentLimit, Dnf.DefaultConcurrentLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in Dnf.Chunk(ids, 200 * Dnf.CompositeQueryLimit).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync().ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in Dnf.Chunk(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                                request.Delete($"{batchNo}_{chunkIdx}", false, chunk.ToArray());
                            Logger?.Invoke($"Delete Start {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Delete End {batchNo * 200 * Dnf.CompositeQueryLimit + batch.Count}");
                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
                return results;
            }
        }

        #endregion
    }
}
