using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DotNetForce
{
    public class CompositeClient : ICompositeClient
    {
        public JsonHttpClient JsonHttp { get; set; }
        public string ApiVersion { get; set; }
        public Action<string> Logger { get; set; }

        public CompositeClient(JsonHttpClient jsonHttp, string apiVersion)
            : this(jsonHttp, apiVersion, null)
        {
        }

        public CompositeClient(JsonHttpClient jsonHttp, string apiVersion, Action<string> logger)
        {
            JsonHttp = jsonHttp;
            ApiVersion = apiVersion;
            Logger = logger;
        }

        #region Collections

        public Task<CompositeResult> CreateAsync<T>(IEnumerable<T> records)
        {
            return CreateAsync(records, false);
        }

        public async Task<CompositeResult> CreateAsync<T>(IEnumerable<T> records, bool allOrNone)
        {
            if (records == null) throw new ArgumentNullException("records");

            if (allOrNone)
            {
                if (records.Count() > 200 * DNF.COMPOSITE_QUERY_LIMIT) throw new ArgumentOutOfRangeException("records");

                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in DNF.Chunk(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                {
                    request.Create($"{chunkIdx}", allOrNone, chunk);
                }
                Logger?.Invoke($"Create Start");
                var results = await PostAsync(request).ConfigureAwait(false);
                Logger?.Invoke($"Create End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(DNF.DEFAULT_CONCURRENT_LIMIT, DNF.DEFAULT_CONCURRENT_LIMIT);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in DNF.Chunk(records, 200 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync().ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in DNF.Chunk(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            {
                                request.Create($"{batchNo}_{chunkIdx}", allOrNone, chunk);
                            }
                            Logger?.Invoke($"Create Start {batchNo * 200 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Create End {batchNo * 200 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");

                            if (batchNo > 0)
                            {
                                await tasks[batchNo - 1].ConfigureAwait(false);
                            }

                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                return results;
            }
        }

        public async Task<CompositeResult> RetrieveAsync(string objectName, IEnumerable<string> ids, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (ids == null) throw new ArgumentNullException("ids");
            if (fields == null || fields.Length == 0) throw new ArgumentNullException("fields");

            var throttler = new SemaphoreSlim(DNF.DEFAULT_CONCURRENT_LIMIT, DNF.DEFAULT_CONCURRENT_LIMIT);
            var tasks = new List<Task>();
            var results = new CompositeResult();

            foreach (var (batch, batchNo) in DNF.Chunk(ids, 2000 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
            {
                await throttler.WaitAsync().ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var request = new CompositeRequest();
                        foreach (var (chunk, chunkIdx) in DNF.Chunk(batch, 2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                        {
                            request.Retrieve($"{batchNo}_{chunkIdx}", objectName, chunk, fields);
                        }
                        Logger?.Invoke($"Retrieve Start {batchNo * 2000 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");
                        var result = await PostAsync(request).ConfigureAwait(false);
                        Logger?.Invoke($"Retrieve End {batchNo * 2000 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");

                        if (batchNo > 0)
                        {
                            await tasks[batchNo - 1].ConfigureAwait(false);
                        }

                        results.Add(result);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        public async Task<CompositeResult> RetrieveExternalAsync(string objectName, string externalFieldName, IEnumerable<string> externalIds, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException("externalFieldName");
            if (externalIds == null || !externalIds.Any()) throw new ArgumentNullException("externalIds");
            if (fields == null || fields.Length == 0) throw new ArgumentNullException("fields");

            var throttler = new SemaphoreSlim(DNF.DEFAULT_CONCURRENT_LIMIT, DNF.DEFAULT_CONCURRENT_LIMIT);
            var tasks = new List<Task>();
            var results = new CompositeResult();

            foreach (var (batch, batchNo) in DNF.Chunk(externalIds, 2000 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
            {
                await throttler.WaitAsync().ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var request = new CompositeRequest();
                        foreach (var (chunk, chunkIdx) in DNF.Chunk(batch, 2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                        {
                            request.RetrieveExternal($"{batchNo}_{chunkIdx}", objectName, externalFieldName, chunk, fields);
                        }
                        Logger?.Invoke($"Retrieve Start {batchNo * 2000 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");
                        var result = await PostAsync(request).ConfigureAwait(false);
                        Logger?.Invoke($"Retrieve End {batchNo * 2000 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");

                        if (batchNo > 0)
                        {
                            await tasks[batchNo - 1].ConfigureAwait(false);
                        }

                        results.Add(result);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        public Task<CompositeResult> UpdateAsync<T>(IEnumerable<T> records)
        {
            return UpdateAsync(records, false);
        }

        public async Task<CompositeResult> UpdateAsync<T>(IEnumerable<T> records, bool allOrNone)
        {
            if (records == null) throw new ArgumentNullException("records");

            if (allOrNone)
            {
                if (records.Count() > 200 * DNF.COMPOSITE_QUERY_LIMIT) throw new ArgumentOutOfRangeException("records");

                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in DNF.Chunk(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                {
                    request.Update($"{chunkIdx}", allOrNone, chunk);
                }
                Logger?.Invoke($"Update Start");
                var results = await PostAsync(request).ConfigureAwait(false);
                Logger?.Invoke($"Update End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(DNF.DEFAULT_CONCURRENT_LIMIT, DNF.DEFAULT_CONCURRENT_LIMIT);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in DNF.Chunk(records, 200 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync().ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in DNF.Chunk(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            {
                                request.Update($"{batchNo}_{chunkIdx}", allOrNone, chunk);
                            }
                            Logger?.Invoke($"Update Start {batchNo * 200 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Update End {batchNo * 200 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");

                            if (batchNo > 0)
                            {
                                await tasks[batchNo - 1].ConfigureAwait(false);
                            }

                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                return results;
            }
        }

        //public Task<CompositeResult> UpsertExternalAsync<T>(string externalFieldName, IEnumerable<T> records)
        //{
        //    return UpsertExternalAsync(externalFieldName, records, false);
        //}

        //public async Task<CompositeResult> UpsertExternalAsync<T>(string externalFieldName, IEnumerable<T> records, bool allOrNone)
        //{
        //    if (records == null) throw new ArgumentNullException("records");

        //    if (allOrNone)
        //    {
        //        if (records.Count() > 200 * DNF.COMPOSITE_QUERY_LIMIT) throw new ArgumentOutOfRangeException("records");

        //        var request = new CompositeRequest();
        //        foreach (var (chunk, chunkIdx) in DNF.Chunk(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
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
        //        var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
        //        var tasks = new List<Task>();
        //        var results = new CompositeResult();

        //        foreach (var (batch, batchNo) in DNF.Chunk(records, 200 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
        //        {
        //            await throttler.WaitAsync().ConfigureAwait(false);
        //            tasks.Add(Task.Run(async () =>
        //            {
        //                try
        //                {
        //                    var request = new CompositeRequest();
        //                    foreach (var (chunk, chunkIdx) in DNF.Chunk(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
        //                    {
        //                        request.UpsertExternal($"{batchNo}_{chunkIdx}", allOrNone, externalFieldName, chunk);
        //                    }
        //                    Logger?.Invoke($"Update Start {batchNo * 200 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");
        //                    var result = await PostAsync(request).ConfigureAwait(false);
        //                    Logger?.Invoke($"Update End {batchNo * 200 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");

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

        //        await Task.WhenAll(tasks);
        //        return results;
        //    }
        //}

        public Task<CompositeResult> DeleteAsync(IEnumerable<string> ids)
        {
            return DeleteAsync(ids, false);
        }

        public async Task<CompositeResult> DeleteAsync(IEnumerable<string> ids, bool allOrNone)
        {
            if (ids == null) throw new ArgumentNullException("ids");

            if (allOrNone)
            {
                if (ids.Count() > 200 * DNF.COMPOSITE_QUERY_LIMIT) throw new ArgumentOutOfRangeException("records");

                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in DNF.Chunk(ids, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                {
                    request.Delete($"{chunkIdx}", allOrNone, chunk.ToArray());
                }
                Logger?.Invoke($"Delete Start");
                var results = await PostAsync(request).ConfigureAwait(false);
                Logger?.Invoke($"Delete End");
                return results;
            }
            else
            {
                var throttler = new SemaphoreSlim(DNF.DEFAULT_CONCURRENT_LIMIT, DNF.DEFAULT_CONCURRENT_LIMIT);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in DNF.Chunk(ids, 200 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
                {
                    await throttler.WaitAsync().ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in DNF.Chunk(batch, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            {
                                request.Delete($"{batchNo}_{chunkIdx}", allOrNone, chunk.ToArray());
                            }
                            Logger?.Invoke($"Delete Start {batchNo * 200 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Delete End {batchNo * 200 * DNF.COMPOSITE_QUERY_LIMIT + batch.Count}");
                            results.Add(result);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                return results;
            }
        }

        #endregion

        public async Task<CompositeResult> PostAsync(ICompositeRequest request)
        {
            if (request == null || request.CompositeRequests.Count <= 0) throw new ArgumentNullException("request");

            try
            {
                var urlSuffix = $"{request.Prefix}composite";

                if (request.AllOrNone)
                {
                    var requests = request.CompositeRequests;

                    if (requests.Count > DNF.COMPOSITE_LIMIT) throw new ArgumentOutOfRangeException("request");
                    if (requests.Count(c => IsQuery(c.ResponseType)) > DNF.COMPOSITE_QUERY_LIMIT) throw new ArgumentOutOfRangeException("request");

                    var inputObject = new JObject
                    {
                        ["allOrNone"] = true,
                        ["compositeRequest"] = JArray.FromObject(requests.Select(req => DNF.Assign(JObject.FromObject(req), new JObject
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
                    var throttler = new SemaphoreSlim(DNF.DEFAULT_CONCURRENT_LIMIT, DNF.DEFAULT_CONCURRENT_LIMIT);
                    var results = new CompositeResult();

                    var chunks = new List<List<CompositeSubrequest>>();
                    List<CompositeSubrequest> chunk = null;

                    foreach (var req in request.CompositeRequests)
                    {
                        var added = false;

                        if (IsQuery(req.ResponseType))
                        {
                            if (chunk != null && chunk.Count(c => IsQuery(c.ResponseType)) < DNF.COMPOSITE_QUERY_LIMIT)
                            {
                                chunk.Add(req);
                                added = true;
                            }
                        }
                        else if (chunk != null && chunk.Count < DNF.COMPOSITE_LIMIT)
                        {
                            chunk.Add(req);
                            added = true;
                        }

                        if (!added)
                        {
                            chunk = new List<CompositeSubrequest> { req };
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
                                    ["compositeRequest"] = JArray.FromObject(requests.Select(req => DNF.Assign(JObject.FromObject(req), new JObject
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
                                var responses = requests.Select(req => new CompositeSubrequestResult
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
                var responses = request.CompositeRequests.Select(req => new CompositeSubrequestResult
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
            if (request == null || request.BatchRequests.Count <= 0) throw new ArgumentNullException("request");

            try
            {
                var urlSuffix = $"{request.Prefix}composite/batch";

                if (request.HaltOnError)
                {
                    if (request.BatchRequests.Count > DNF.BATCH_LIMIT) throw new ArgumentOutOfRangeException("request");

                    var inputObject = new JObject
                    {
                        ["batchRequests"] = JArray.FromObject(request.BatchRequests.Select(req => DNF.Assign(JObject.FromObject(req), new JObject
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
                    var throttler = new SemaphoreSlim(DNF.DEFAULT_CONCURRENT_LIMIT, DNF.DEFAULT_CONCURRENT_LIMIT);
                    var results = new BatchResult();

                    var chunks = new List<List<BatchSubrequest>>();
                    List<BatchSubrequest> chunk = null;

                    foreach (var req in request.BatchRequests)
                    {
                        var added = false;

                        if (chunk != null && chunk.Count < DNF.BATCH_LIMIT)
                        {
                            chunk.Add(req);
                            added = true;
                        }

                        if (!added)
                        {
                            chunk = new List<BatchSubrequest> { req };
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
                                    ["batchRequests"] = JArray.FromObject(requests.Select(req => DNF.Assign(JObject.FromObject(req), new JObject
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
                                var responses = requests.Select(req => new BatchSubrequestResult
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
                var responses = request.BatchRequests.Select(req => new BatchSubrequestResult
                {
                    Result = body,
                    StatusCode = 500
                }).ToList();
                var results = new BatchResult(request.BatchRequests, responses);
                return results;
            }
        }

        public async Task<SaveResponse> CreateTreeAsync<T>(string objectName, IEnumerable<T> objectTree)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            if (objectTree == null || !objectTree.Any()) throw new ArgumentNullException("objectTree");

            if (typeof(IAttributedObject).IsAssignableFrom(typeof(T)))
            {
                return await DNF.TryDeserializeObject(Task.Run(async () =>
                {
                    var result = await JsonHttp.HttpPostAsync<SaveResponse>(
                        new CreateRequest { Records = objectTree.Cast<IAttributedObject>().ToList() },
                        $"composite/tree/{objectName}").ConfigureAwait(false);
                    return result;
                }));
            }
            else
            {
                return await DNF.TryDeserializeObject(Task.Run(async () =>
                {
                    var result = await JsonHttp.HttpPostAsync<SaveResponse>(
                        new JObject { ["records"] = JArray.FromObject(objectTree) },
                        $"composite/tree/{objectName}").ConfigureAwait(false);
                    return result;
                }));
            }
        }

        protected string DecodeReference(string value)
        {
            var pattern = HttpUtility.UrlEncode("@{") +
                $"[0-9a-z](?:[_.0-9a-z]|{HttpUtility.UrlEncode("[")}|{HttpUtility.UrlEncode("]")})*" + //$".+" +
                HttpUtility.UrlEncode("}");
            MatchEvaluator evaluator = m => Uri.UnescapeDataString(m.Value);
            var decoded = Regex.Replace(value ?? "", pattern, evaluator, RegexOptions.IgnoreCase);
            return decoded;
        }
    }
}
