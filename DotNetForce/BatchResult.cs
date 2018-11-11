using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetForce
{
    public class BatchResult
    {
        protected List<BatchSubrequest> _Requests = new List<BatchSubrequest>();
        public List<BatchSubrequest> Requests() => _Requests;

        protected Dictionary<string, ErrorResponses> _Errors = new Dictionary<string, ErrorResponses>();
        public Dictionary<string, ErrorResponses> Errors() => _Errors;
        public ErrorResponses Errors(string referenceId)
        {
            return !_Errors.TryGetValue(referenceId, out ErrorResponses value) ? null : value;
        }

        protected Dictionary<string, QueryResult<JObject>> _Queries = new Dictionary<string, QueryResult<JObject>>();
        public Dictionary<string, QueryResult<JObject>> Queries() => _Queries;
        public QueryResult<JObject> Queries(string referenceId)
        {
            return !_Queries.TryGetValue(referenceId, out QueryResult<JObject> value) ? null : value;
        }

        protected Dictionary<string, JToken> _Results = new Dictionary<string, JToken>();
        public Dictionary<string, JToken> Results() => _Results;
        public JToken Results(string referenceId)
        {
            return !_Results.TryGetValue(referenceId, out JToken value) ? null : value;
        }
        public Dictionary<string, SuccessResponse> SuccessResponses()
        {
            return _Results
                .Where(r => DNF.IsQueryResult(r.Value))
                .ToDictionary(r => r.Key, r => r.Value.ToObject<SuccessResponse>());
        }

        public SuccessResponse SuccessResponses(string referenceId)
        {
            return !_Results.TryGetValue(referenceId, out JToken value) ? default(SuccessResponse)
                : value == null ? default(SuccessResponse)
                : DNF.IsSuccessResponse(value) ? value.ToObject<SuccessResponse>()
                : default(SuccessResponse);
        }

        public BatchResult()
        {
        }

        public BatchResult(List<BatchSubrequest> subrequests, List<BatchSubrequestResult> responses)
        {
            Add(subrequests, responses);
        }

        public void Add(BatchResult result)
        {
            _Requests.AddRange(result._Requests);
            foreach (var item in result._Errors)
            {
                _Errors.Add(item.Key, item.Value);
            }
            foreach (var item in result._Queries)
            {
                _Queries.Add(item.Key, item.Value);
            }
            foreach (var item in result._Results)
            {
                _Results.Add(item.Key, item.Value);
            }
        }

        public void Add(List<BatchSubrequest> subrequests, List<BatchSubrequestResult> responses)
        {
            if (subrequests == null) throw new ArgumentNullException("subrequests");
            if (responses == null) throw new ArgumentNullException("responses");

            for (var i = 0; i < subrequests.Count; i++)
            {
                var subrequest = subrequests[i];
                _Requests.Add(subrequest);

                if (i >= responses.Count)
                {
                    break;
                }

                var response = responses[i];
                var refId = $"{i}";

                switch (subrequest.ResponseType)
                {
                    case "query":
                        try
                        {
                            if (IsSuccessStatusCode(response.StatusCode))
                            {
                                if (DNF.IsQueryResult(response.Result))
                                {
                                    _Queries.Add(refId, response.Result?.ToObject<QueryResult<JObject>>() ?? new QueryResult<JObject>());
                                }
                                else if (response.Result?.Type == JTokenType.Array)
                                {
                                    _Results.Add(refId, (JArray)response.Result);
                                }
                                else
                                {
                                    _Results.Add(refId, new JArray { response.Result });
                                }
                            }
                            else
                            {
                                _Errors.Add(refId, DNF.GetErrorResponses(response.Result));
                            }
                        }
                        catch
                        {
                            _Errors.Add(refId, DNF.GetErrorResponses(response.Result));
                        }
                        break;
                    case "object":
                    default:
                        try
                        {
                            if (IsSuccessStatusCode(response.StatusCode))
                            {
                                _Results.Add(refId, response.Result);
                            }
                            else
                            {
                                _Errors.Add(refId, DNF.GetErrorResponses(response.Result));
                            }
                        }
                        catch
                        {
                            _Errors.Add(refId, DNF.GetErrorResponses(response.Result));
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
