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

        protected Dictionary<int, ErrorResponses> _Errors = new Dictionary<int, ErrorResponses>();
        public Dictionary<int, ErrorResponses> Errors() => _Errors;
        public ErrorResponses Errors(int referenceId)
        {
            return !_Errors.TryGetValue(referenceId, out ErrorResponses value) ? null : value;
        }

        protected Dictionary<int, QueryResult<JObject>> _Queries = new Dictionary<int, QueryResult<JObject>>();
        public Dictionary<int, QueryResult<JObject>> Queries() => _Queries;
        public QueryResult<JObject> Queries(int referenceId)
        {
            return !_Queries.TryGetValue(referenceId, out QueryResult<JObject> value) ? null : value;
        }

        protected Dictionary<int, JToken> _Results = new Dictionary<int, JToken>();
        public Dictionary<int, JToken> Results() => _Results;
        public JToken Results(int referenceId)
        {
            return !_Results.TryGetValue(referenceId, out JToken value) ? null : value;
        }
        public SuccessResponse Results<SuccessResponse>(int referenceId)
        {
            return !_Results.TryGetValue(referenceId, out JToken value) ? default(SuccessResponse) : value == null ? default(SuccessResponse) : value.ToObject<SuccessResponse>();
        }
        
        public Dictionary<int, SuccessResponse> SuccessResponses()
        {
            return _Results
                .Where(r => !string.IsNullOrEmpty(r.Value["id"]?.ToObject<string>())
                && r.Value["success"]?.Type == JTokenType.Boolean
                && r.Value["errors"]?.Type == JTokenType.Array)
                .ToDictionary(r => r.Key, r => r.Value.ToObject<SuccessResponse>());
        }

        public SuccessResponse SuccessResponses(int referenceId)
        {
            return !_Results.TryGetValue(referenceId, out JToken value) ? default(SuccessResponse)
                : value == null ? default(SuccessResponse)
                : !string.IsNullOrEmpty(value["id"]?.ToObject<string>())
                && value["success"]?.Type == JTokenType.Boolean
                && value["errors"]?.Type == JTokenType.Array ? value.ToObject<SuccessResponse>()
                : default(SuccessResponse);
        }

        public BatchResult()
        {
        }

        public BatchResult(List<BatchSubrequest> subrequests, List<BatchSubrequestResult> responses)
        {
            Add(subrequests, responses);
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

                switch (subrequest.ResponseType)
                {
                    case "query":
                        try
                        {
                            if (IsSuccessStatusCode(response.StatusCode))
                            {

                                if (response.Result?.Type == JTokenType.Object &&
                                    response.Result?["totalSize"]?.Type == JTokenType.Integer &&
                                    response.Result?["done"]?.Type == JTokenType.Boolean &&
                                    response.Result?["records"]?.Type == JTokenType.Array)
                                {
                                    _Queries.Add(i, response.Result?.ToObject<QueryResult<JObject>>() ?? new QueryResult<JObject>());
                                }
                                else
                                {
                                    _Errors.Add(i, DNF.GetErrorResponses(response.Result));
                                }
                            }
                            else
                            {
                                _Errors.Add(i, DNF.GetErrorResponses(response.Result));
                            }
                        }
                        catch
                        {
                            _Errors.Add(i, DNF.GetErrorResponses(response.Result));
                        }
                        break;
                    case "object":
                    default:
                        try
                        {
                            if (IsSuccessStatusCode(response.StatusCode))
                            {
                                _Results.Add(i, response.Result ?? new JObject());
                            }
                            else
                            {
                                _Errors.Add(i, DNF.GetErrorResponses(response.Result));
                            }
                        }
                        catch
                        {
                            _Errors.Add(i, DNF.GetErrorResponses(response.Result));
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
            if (_Errors.Count > 0) output["Errors"] = JObject.FromObject(_Errors);
            if (_Queries.Count > 0) output["Queries"] = JObject.FromObject(_Queries);
            if (_Results.Count > 0) output["Objects"] = JObject.FromObject(_Results);
            if (_Requests.Count > 0) output["Requests"] = JObject.FromObject(_Requests);
            return output.ToString();
        }
    }
}
