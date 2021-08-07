using System;
using System.Collections.Generic;
using System.Linq;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
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

        public CompositeResult(IList<CompositeSubRequest> subRequests, IList<CompositeSubRequestResult> responses)
        {
            Add(subRequests, responses);
        }

        public IList<CompositeSubRequest> Requests()
        {
            return _Requests;
        }

        public IDictionary<string, ErrorResponses> Errors()
        {
            return _Errors;
        }

        public ErrorResponses Errors(string referenceId)
        {
            return !_Errors.TryGetValue(referenceId, out var value) ? null : value;
        }

        public IDictionary<string, QueryResult<JObject>> Queries()
        {
            return Queries<JObject>();
        }

        public IDictionary<string, QueryResult<T>> Queries<T>()
        {
            return _Queries.ToDictionary(q => q.Key, q => q.Value.ToObject<QueryResult<T>>());
        }

        public QueryResult<JObject> Queries(string referenceId)
        {
            return Queries<JObject>(referenceId);
        }

        public QueryResult<T> Queries<T>(string referenceId)
        {
            return !_Queries.TryGetValue(referenceId, out var value) ? null : value.ToObject<QueryResult<T>>();
        }

        public IDictionary<string, JToken> Results()
        {
            return _Results;
        }

        public JToken Results(string referenceId)
        {
            return !_Results.TryGetValue(referenceId, out var value) ? null : value;
        }

        public IDictionary<string, SuccessResponse> SuccessResponses()
        {
            return _Results
                .Where(r => Dnf.IsSuccessResponse(r.Value))
                .ToDictionary(r => r.Key, r => r.Value.ToObject<SuccessResponse>());
        }

        public SuccessResponse SuccessResponses(string referenceId)
        {
            return !_Results.TryGetValue(referenceId, out var value) ? default
                : value == null ? default
                : Dnf.IsSuccessResponse(value) ? value.ToObject<SuccessResponse>()
                : default;
        }

        public void Add(CompositeResult result)
        {
            foreach (var request in result._Requests) _Requests.Add(request);
            foreach (var item in result._Errors) _Errors.Add(item.Key, item.Value);
            foreach (var item in result._Queries) _Queries.Add(item.Key, item.Value);
            foreach (var item in result._Results) _Results.Add(item.Key, item.Value);
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

                var response = responses[i];

                switch (subRequest.ResponseType)
                {
                    case "collections":
                        try
                        {
                            if (response.Body.Type == JTokenType.Array)
                                foreach (var (row, j) in response.Body.Select((row, j) => (row, j)))
                                {
                                    var refId = $"{subRequest.ReferenceId}_{j}";

                                    if (row?.Type == JTokenType.Object)
                                    {
                                        if (Dnf.IsQueryResult(row))
                                        {
                                            _Queries.Add(refId, (JObject)row ?? new JObject());
                                        }
                                        else if (Dnf.IsSuccessResponse(row))
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
                                            if (row["errors"].Any()) _Errors.Add(refId, Dnf.GetErrorResponses(row["errors"]));
                                        }
                                        else
                                        {
                                            _Errors.Add(refId, Dnf.GetErrorResponses(row));
                                        }
                                    }
                                    else if (row["errors"]?.Type == JTokenType.Array)
                                    {
                                        if (row["errors"].Any()) _Errors.Add(refId, Dnf.GetErrorResponses(row["errors"]));
                                    }
                                    else
                                    {
                                        _Errors.Add(refId, Dnf.GetErrorResponses(row));
                                    }
                                }
                            else _Errors.Add(subRequest.ReferenceId, Dnf.GetErrorResponses(response.Body));
                        }
                        catch
                        {
                            _Errors.Add(subRequest.ReferenceId, Dnf.GetErrorResponses(response.Body));
                        }
                        break;
                    case "query":
                        try
                        {
                            if (IsSuccessStatusCode(response.HttpStatusCode))
                            {
                                if (Dnf.IsQueryResult(response.Body)) _Queries.Add(subRequest.ReferenceId, (JObject)response.Body ?? new JObject());
                                else if (response.Body?.Type == JTokenType.Array) _Results.Add(subRequest.ReferenceId, response.Body);
                                else _Results.Add(subRequest.ReferenceId, new JArray { response.Body });
                            }
                            else
                            {
                                _Errors.Add(subRequest.ReferenceId, Dnf.GetErrorResponses(response.Body));
                            }
                        }
                        catch
                        {
                            _Errors.Add(subRequest.ReferenceId, Dnf.GetErrorResponses(response.Body));
                        }
                        break;
                    default:
                        try
                        {
                            if (IsSuccessStatusCode(response.HttpStatusCode)) _Results.Add(subRequest.ReferenceId, response.Body);
                            else _Errors.Add(subRequest.ReferenceId, Dnf.GetErrorResponses(response.Body));
                        }
                        catch
                        {
                            _Errors.Add(subRequest.ReferenceId, Dnf.GetErrorResponses(response.Body));
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

        public string ToString(bool includeRequests)
        {
            return ToString(includeRequests, Formatting.Indented);
        }

        public string ToString(bool includeRequests, Formatting formatting)
        {
            var output = new JObject();
            if (_Errors.Count > 0) output["Errors"] = JToken.FromObject(_Errors);
            if (_Queries.Count > 0) output["Queries"] = JToken.FromObject(_Queries);
            if (_Results.Count > 0) output["Objects"] = JToken.FromObject(_Results);
            if (includeRequests && _Requests.Count > 0) output["Requests"] = JToken.FromObject(_Requests);
            return output.ToString(formatting);
        }
    }
}
