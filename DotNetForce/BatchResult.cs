using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForce
{
    public class BatchResult
    {
        // ReSharper disable once InconsistentNaming
        protected readonly IDictionary<string, ErrorResponses?> _Errors = new Dictionary<string, ErrorResponses?>();
        
        // ReSharper disable once InconsistentNaming
        protected readonly IDictionary<string, JObject> _Queries = new Dictionary<string, JObject>();
        
        // ReSharper disable once InconsistentNaming
        protected readonly IList<BatchSubRequest> _Requests = new List<BatchSubRequest>();

        // ReSharper disable once InconsistentNaming
        protected readonly IDictionary<string, JToken> _Results = new Dictionary<string, JToken>();

        public BatchResult() { }

        public BatchResult(IList<BatchSubRequest> subRequests, IList<BatchSubRequestResult> responses)
        {
            Add(subRequests, responses);
        }

        public IEnumerable<BatchSubRequest> Requests()
        {
            return _Requests;
        }

        public IDictionary<string, ErrorResponses?> Errors()
        {
            return _Errors;
        }

        public ErrorResponses? Errors(string referenceId)
        {
            return _Errors.TryGetValue(referenceId, out var value) ? null : value;
        }

        public IDictionary<string, QueryResult<JObject>> Queries()
        {
            return Queries<JObject>();
        }

        public IDictionary<string, QueryResult<T>> Queries<T>()
        {
            return _Queries.ToDictionary(q => q.Key, q => q.Value.ToObject<QueryResult<T>>() ?? new QueryResult<T>());
        }

        public QueryResult<JObject> Queries(string referenceId)
        {
            return Queries<JObject>(referenceId);
        }

        public QueryResult<T> Queries<T>(string referenceId)
        {
            return !_Queries.TryGetValue(referenceId, out var value) ? new QueryResult<T>() : value.ToObject<QueryResult<T>>() ?? new QueryResult<T>();
        }

        public IDictionary<string, JToken> Results()
        {
            return _Results;
        }

        public JToken Results(string referenceId)
        {
            return (!_Results.TryGetValue(referenceId, out var value) ? null : value) ?? JValue.CreateNull();
        }

        public IDictionary<string, SuccessResponse> SuccessResponses()
        {
            return _Results
                .Where(r => QueryResult<JToken>.IsAssignableFrom(r.Value))
                .ToDictionary(r => r.Key, r => r.Value.ToObject<SuccessResponse>() ?? new SuccessResponse());
        }

        public SuccessResponse SuccessResponses(string referenceId)
        {
            return (!_Results.TryGetValue(referenceId, out var value) ? new SuccessResponse()
                : value == null ? new SuccessResponse()
                : SuccessResponse.TryCast(value)) ?? new SuccessResponse();
        }

        public void Add(BatchResult result)
        {
            foreach (var request in result._Requests) _Requests.Add(request);
            foreach (var (key, value) in result._Errors) _Errors.Add(key, value);
            foreach (var (key, value) in result._Queries) _Queries.Add(key, value);
            foreach (var (key, value) in result._Results) _Results.Add(key, value);
        }

        public void Add(IList<BatchSubRequest> subRequests, IList<BatchSubRequestResult> responses)
        {
            if (subRequests == null) throw new ArgumentNullException(nameof(subRequests));
            if (responses == null) throw new ArgumentNullException(nameof(responses));

            for (var i = 0; i < subRequests.Count; i++)
            {
                var subRequest = subRequests[i];
                _Requests.Add(subRequest);

                if (i >= responses.Count) break;

                var response = responses[i];
                var refId = $"{i}";

                switch (subRequest.ResponseType)
                {
                    case "query":
                        try
                        {
                            if (IsSuccessStatusCode(response.StatusCode))
                            {
                                if (QueryResult<JToken>.IsAssignableFrom(response.Result)) _Queries.Add(refId, response.Result as JObject ?? new JObject());
                                else if (response.Result?.Type == JTokenType.Array) _Results.Add(refId, response.Result);
                                else _Results.Add(refId, new JArray { response.Result ?? JValue.CreateNull() });
                            }
                            else
                            {
                                _Errors.Add(refId, ErrorResponse.TryCast(response.Result));
                            }
                        }
                        catch
                        {
                            _Errors.Add(refId, ErrorResponse.TryCast(response.Result));
                        }
                        break;
                    default:
                        try
                        {
                            if (IsSuccessStatusCode(response.StatusCode)) _Results.Add(refId, response.Result ?? JValue.CreateNull());
                            else _Errors.Add(refId, ErrorResponse.TryCast(response.Result));
                        }
                        catch
                        {
                            _Errors.Add(refId, ErrorResponse.TryCast(response.Result));
                        }
                        break;
                }
            }

            bool IsSuccessStatusCode(int statusCode)
            {
                return statusCode >= 200 && statusCode <= 299;
            }
        }

        public override string ToString()
        {
            var output = new JObject();
            if (_Errors.Count > 0) output["Errors"] = JToken.FromObject(_Errors);
            if (_Queries.Count > 0) output["Queries"] = JToken.FromObject(_Queries);
            if (_Results.Count > 0) output["Objects"] = JToken.FromObject(_Results);
            //if (_Requests.Count > 0) output["Requests"] = JToken.FromObject(_Requests);
            return output.ToString();
        }

        public string ToString(bool includeRequests, Formatting formatting = Formatting.Indented)
        {
            var output = new JObject();
            if (_Errors.Count > 0) output["Errors"] = JToken.FromObject(_Errors);
            if (_Queries.Count > 0) output["Queries"] = JToken.FromObject(_Queries);
            if (_Results.Count > 0) output["Objects"] = JToken.FromObject(_Results);
            if (includeRequests && _Requests.Count > 0) output["Requests"] = JToken.FromObject(_Requests);
            return output.ToString(formatting);
        }

        public BatchResult Assert()
        {
            var exList = new List<ForceException>();
            foreach (var (key, value) in Errors())
            {
                var request = Requests().Where((req, reqIdx) => $"{reqIdx}" == key).FirstOrDefault();
                exList.Add(new ForceException(
                    value?.Select(error => error.ErrorCode).FirstOrDefault() ?? $"{Error.Unknown}",
                    (request == null ? $"{key}:" : $"{request}") + Environment.NewLine +
                    string.Join(Environment.NewLine, value?.Select(error => error.Message) ?? Enumerable.Empty<string>())
                ));
            }
            if (exList.Count > 0) throw new AggregateException(exList);
            return this;
        }
    }
}
