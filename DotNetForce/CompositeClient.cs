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
                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in records.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
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
                var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in records.Chunk(200 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await throttler.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in batch.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            {
                                request.Create($"{batchNo}/{chunkIdx}", allOrNone, chunk);
                            }
                            Logger?.Invoke($"Create Start {batchNo}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Create End {batchNo}");

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

            var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
            var tasks = new List<Task>();
            var results = new CompositeResult();

            foreach (var (batch, batchNo) in ids.Chunk(2000 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await throttler.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        var request = new CompositeRequest();
                        foreach (var (chunk, chunkIdx) in batch.Chunk(2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                        {
                            request.Retrieve($"{batchNo}/{chunkIdx}", objectName, chunk, fields);
                        }
                        Logger?.Invoke($"Retrieve Start {batchNo}");
                        var result = await PostAsync(request).ConfigureAwait(false);
                        Logger?.Invoke($"Retrieve End {batchNo}");

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

        public async Task<CompositeResult> RetrieveExternalAsync(string objectName, string externalFieldName,  IEnumerable<string> externalIds, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException("externalFieldName");
            if (externalIds == null || !externalIds.Any()) throw new ArgumentNullException("externalIds");
            if (fields == null || fields.Length == 0) throw new ArgumentNullException("fields");
            
            var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
            var tasks = new List<Task>();
            var results = new CompositeResult();

            foreach (var (batch, batchNo) in externalIds.Chunk(2000 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await throttler.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        var request = new CompositeRequest();
                        foreach (var (chunk, chunkIdx) in batch.Chunk(2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                        {
                            request.RetrieveExternal($"{batchNo}/{chunkIdx}", objectName, externalFieldName, chunk, fields);
                        }
                        Logger?.Invoke($"Retrieve Start {batchNo}");
                        var result = await PostAsync(request).ConfigureAwait(false);
                        Logger?.Invoke($"Retrieve End {batchNo}");

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
                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in records.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
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
                var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in records.Chunk(200 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await throttler.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in batch.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            {
                                request.Update($"{batchNo}/{chunkIdx}", allOrNone, chunk);
                            }
                            Logger?.Invoke($"Update Start {batchNo}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Update End {batchNo}");

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
        //        var request = new CompositeRequest();
        //        foreach (var (chunk, chunkIdx) in records.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
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

        //        foreach (var (batch, batchNo) in records.Chunk(200 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
        //        {
        //            tasks.Add(Task.Run(async () =>
        //            {
        //                await throttler.WaitAsync().ConfigureAwait(false);
        //                try
        //                {
        //                    var request = new CompositeRequest();
        //                    foreach (var (chunk, chunkIdx) in batch.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
        //                    {
        //                        request.UpsertExternal($"{batchNo}/{chunkIdx}", allOrNone, externalFieldName, chunk);
        //                    }
        //                    Logger?.Invoke($"Update Start {batchNo}");
        //                    var result = await PostAsync(request).ConfigureAwait(false);
        //                    Logger?.Invoke($"Update End {batchNo}");

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
                var request = new CompositeRequest();
                foreach (var (chunk, chunkIdx) in ids.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
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
                var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
                var tasks = new List<Task>();
                var results = new CompositeResult();

                foreach (var (batch, batchNo) in ids.Chunk(200 * DNF.COMPOSITE_QUERY_LIMIT).Select((batch, batchNo) => (batch, batchNo)))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await throttler.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            var request = new CompositeRequest();
                            foreach (var (chunk, chunkIdx) in batch.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
                            {
                                request.Delete($"{batchNo}/{chunkIdx}", allOrNone, chunk.ToArray());
                            }
                            Logger?.Invoke($"Delete Start {batchNo}");
                            var result = await PostAsync(request).ConfigureAwait(false);
                            Logger?.Invoke($"Delete End {batchNo}");
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
                        ["compositeRequest"] = JArray.FromObject(requests.Select(req => JObject.FromObject(req).Assign(new JObject
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
                    var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
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

                    foreach (var requests in chunks)
                    {
                        await throttler.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            var inputObject = new JObject
                            {
                                ["compositeRequest"] = JArray.FromObject(requests.Select(req => JObject.FromObject(req).Assign(new JObject
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
                    }
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
                        ["batchRequests"] = JArray.FromObject(request.BatchRequests.Select(req => JObject.FromObject(req).Assign(new JObject
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
                    var throttler = new SemaphoreSlim(DNF.ConcurrentRequestLimit, DNF.ConcurrentRequestLimit);
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

                    foreach (var requests in chunks)
                    {
                        await throttler.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            var inputObject = new JObject
                            {
                                ["batchRequests"] = JArray.FromObject(requests.Select(req => JObject.FromObject(req).Assign(new JObject
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
                    }
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

        public async Task<SaveResponse> CreateTreeAsync<JObject>(string objectName, IEnumerable<JObject> objectTree)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            if (objectTree == null) throw new ArgumentNullException("objectTree");

            return await DNF.TryDeserializeObject(async () =>
            {
                var result = await JsonHttp.HttpPostAsync<SaveResponse>(
                    new CreateRequest { Records = objectTree.Cast<IAttributedObject>().ToList() },
                    $"composite/tree/{objectName}").ConfigureAwait(false);
                return result;
            });
        }

        protected string DecodeReference(string value)
        {
            var pattern = Uri.EscapeDataString("@{") +
                $"[0-9a-z](?:[_.0-9a-z]|{Uri.EscapeDataString("[")}|{Uri.EscapeDataString("]")})*" + //$".+" +
                Uri.EscapeDataString("}");
            MatchEvaluator evaluator = m => Uri.UnescapeDataString(m.Value);
            var decoded = Regex.Replace(value ?? "", pattern, evaluator, RegexOptions.IgnoreCase);
            return decoded;
        }
    }
}
