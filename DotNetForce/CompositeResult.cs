using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace DotNetForce
{
    public class CompositeResult
    {
        // ReSharper disable once InconsistentNaming
        protected IDictionary<string, ErrorResponses> _Errors = new Dictionary<string, ErrorResponses>();

        // ReSharper disable once InconsistentNaming
        protected IDictionary<string, JObject> _Queries = new Dictionary<string, JObject>();

        // ReSharper disable once InconsistentNaming
        protected IList<CompositeSubRequest> _Requests = new List<CompositeSubRequest>();

        // ReSharper disable once InconsistentNaming
        protected IDictionary<string, JToken> _Results = new Dictionary<string, JToken>();

        public CompositeResult() { }

        public CompositeResult(IList<CompositeSubRequest> subRequests, IList<CompositeSubRequestResult> responses) => Add(subRequests, responses);

        public IList<CompositeSubRequest> Requests() => _Requests;

        public IDictionary<string, ErrorResponses> Errors() => _Errors;

        public ErrorResponses? Errors(string? referenceId) => referenceId != null && _Errors.TryGetValue(referenceId, out var value) ? value : null;

        public IDictionary<string, QueryResult<JObject>> Queries() => Queries<JObject>();

        public IDictionary<string, QueryResult<T>> Queries<T>() =>
            _Queries.ToDictionary(q => q.Key, q => q.Value.ToObject<QueryResult<T>>() ?? new QueryResult<T>());

        public QueryResult<JObject> Queries(string referenceId) => Queries<JObject>(referenceId);

        public QueryResult<T> Queries<T>(string referenceId) =>
            !_Queries.TryGetValue(referenceId, out var value) ? new QueryResult<T>() : value.ToObject<QueryResult<T>>() ?? new QueryResult<T>();

        public IDictionary<string, JToken> Results() => _Results;

        public JToken Results(string referenceId) => !_Results.TryGetValue(referenceId, out var value) ? JValue.CreateNull() : value;

        public IDictionary<string, SuccessResponse> SuccessResponses() =>
            _Results
                .Where(r => SuccessResponse.IsAssignableFrom(r.Value))
                .ToDictionary(
                    r => r.Key,
                    r => r.Value.ToObject<SuccessResponse>() ?? new SuccessResponse());

        public SuccessResponse SuccessResponses(string referenceId) =>
            !_Results.TryGetValue(referenceId, out var value) ? new SuccessResponse()
                : value == null ? new SuccessResponse()
                : SuccessResponse.TryCast(value) ?? new SuccessResponse();

        public void Add(CompositeResult result)
        {
            foreach (var request in result._Requests) _Requests.Add(request);
            foreach (var (key, value) in result._Errors) _Errors.Add(key, value);
            foreach (var (key, value) in result._Queries) _Queries.Add(key, value);
            foreach (var (key, value) in result._Results) _Results.Add(key, value);
        }

        public void Add(IList<CompositeSubRequest> subRequests, IList<CompositeSubRequestResult> responses)
        {
            if (subRequests == null) throw new ArgumentNullException(nameof(subRequests));
            if (responses == null) throw new ArgumentNullException(nameof(responses));

            for (var i = 0; i < subRequests.Count; i++)
            {
                var subRequest = subRequests[i];
                _Requests.Add(subRequest);

                if (i >= responses.Count) break;

                var response = responses[i] ?? new CompositeSubRequestResult();

                switch (subRequest.ResponseType)
                {
                    case "collections":
                        try
                        {
                            if (response.Body?.Type == JTokenType.Array)
                                foreach (var (row, j) in response.Body.Select((row, j) => (row, j)))
                                {
                                    var refId = $"{subRequest.ReferenceId}_{j}";

                                    if (row?.Type == JTokenType.Object)
                                    {
                                        if (QueryResult<JToken>.IsAssignableFrom(row))
                                        {
                                            _Queries.Add(refId, (JObject)row);
                                        }
                                        else if (SuccessResponse.IsAssignableFrom(row))
                                        {
                                            _Results.Add(refId, row);
                                        }
                                        else if (row["attributes"]?.Type == JTokenType.Object)
                                        {
                                            _Results.Add(refId, row);
                                        }
                                        else if ((bool?)row["success"] == true)
                                        {
                                            _Results.Add(refId, row);
                                        }
                                        else if (row["errors"]?.Type == JTokenType.Array)
                                        {
                                            if (row["errors"]?.Any() == true)
                                                _Errors.Add(refId, ErrorResponse.TryCast(row["errors"]));
                                        }
                                        else
                                        {
                                            _Errors.Add(refId, ErrorResponse.TryCast(row));
                                        }
                                    }
                                    else if (row?["errors"]?.Type == JTokenType.Array)
                                    {
                                        if (row["errors"]?.Any() == true) _Errors.Add(refId, ErrorResponse.TryCast(row["errors"]));
                                    }
                                    else
                                    {
                                        _Errors.Add(refId, ErrorResponse.TryCast(row));
                                    }
                                }
                            else
                                _Errors.Add(subRequest.ReferenceId ?? string.Empty, ErrorResponse.TryCast(response.Body));
                        }
                        catch
                        {
                            _Errors.Add(subRequest.ReferenceId ?? string.Empty, ErrorResponse.TryCast(response.Body));
                        }
                        break;
                    case "query":
                        try
                        {
                            if (IsSuccessStatusCode(response.HttpStatusCode))
                            {
                                if (QueryResult<JToken>.IsAssignableFrom(response.Body))
                                    _Queries.Add(subRequest.ReferenceId ?? string.Empty, response.Body as JObject ?? new JObject());
                                else if (response.Body?.Type == JTokenType.Array)
                                    _Results.Add(subRequest.ReferenceId ?? string.Empty, response.Body);
                                else
                                    _Results.Add(subRequest.ReferenceId ?? string.Empty, new JArray { response.Body ?? new JObject() });
                            }
                            else
                            {
                                _Errors.Add(subRequest.ReferenceId ?? string.Empty, ErrorResponse.TryCast(response.Body));
                            }
                        }
                        catch
                        {
                            _Errors.Add(subRequest.ReferenceId ?? string.Empty, ErrorResponse.TryCast(response.Body));
                        }
                        break;
                    default:
                        try
                        {
                            if (IsSuccessStatusCode(response.HttpStatusCode))
                                _Results.Add(subRequest.ReferenceId ?? string.Empty, response.Body ?? new JObject());
                            else
                                _Errors.Add(subRequest.ReferenceId ?? string.Empty, ErrorResponse.TryCast(response.Body));
                        }
                        catch
                        {
                            _Errors.Add(subRequest.ReferenceId ?? string.Empty, ErrorResponse.TryCast(response.Body));
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
        

        public CompositeResult Assert()
        {
            var exList = new List<ForceException>();
            foreach (var (key, value) in Errors())
            {
                var request = Requests().FirstOrDefault(req => req.ReferenceId == key);
                exList.Add(new ForceException(
                    value.Select(error => error.ErrorCode).FirstOrDefault() ?? $"{Error.Unknown}",
                    (request == null ? $"{key}:" : $"{request}") + Environment.NewLine +
                    string.Join(Environment.NewLine, value.Select(error => error.Message))
                ));
            }
            if (exList.Count > 0) throw new ForceException(exList);
            return this;
        }
    }
}
