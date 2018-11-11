using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Force;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DotNetForce
{
    public class CompositeResult
    {
        protected List<CompositeSubrequest> _Requests = new List<CompositeSubrequest>();
        public List<CompositeSubrequest> Requests() => _Requests;

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
                .Where(r => DNF.IsSuccessResponse(r.Value))
                .ToDictionary(r => r.Key, r => r.Value.ToObject<SuccessResponse>());
        }

        public SuccessResponse SuccessResponses(string referenceId)
        {
            return !_Results.TryGetValue(referenceId, out JToken value) ? default(SuccessResponse)
                : value == null ? default(SuccessResponse)
                : DNF.IsSuccessResponse(value) ? value.ToObject<SuccessResponse>()
                : default(SuccessResponse);
        }

        public CompositeResult()
        {
        }

        public CompositeResult(List<CompositeSubrequest> subrequests, List<CompositeSubrequestResult> responses)
        {
            Add(subrequests, responses);
        }

        public void Add(CompositeResult result)
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

        public void Add(List<CompositeSubrequest> subrequests, List<CompositeSubrequestResult> responses)
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
                    case "collections":
                        try
                        {
                            if (response.Body.Type == JTokenType.Array)
                            {
                                foreach (var (row, j) in ((JArray)response.Body).Select((row, j) => (row, j)))
                                {
                                    var refId = $"{subrequest.ReferenceId}_{j}";

                                    if (DNF.IsQueryResult(response.Body))
                                    {
                                        _Queries.Add(refId, row.ToObject<QueryResult<JObject>>());
                                    }
                                    else if (DNF.IsSuccessResponse(row))
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
                                    else if (row["errors"]?.Type != JTokenType.Array)
                                    {
                                        _Errors.Add(refId, DNF.GetErrorResponses(row));
                                    }

                                    if (row["errors"]?.Type == JTokenType.Array && ((JArray)row["errors"]).Count > 0)
                                    {
                                        _Errors.Add(refId, DNF.GetErrorResponses(row["errors"]));
                                    }
                                }
                            }
                            else
                            {
                                _Errors.Add(subrequest.ReferenceId, DNF.GetErrorResponses(response.Body));
                            }
                        }
                        catch
                        {
                            _Errors.Add(subrequest.ReferenceId, DNF.GetErrorResponses(response.Body));
                        }
                        break;
                    case "query":
                        try
                        {
                            if (IsSuccessStatusCode(response.HttpStatusCode))
                            {
                                if (DNF.IsQueryResult(response.Body))
                                {
                                    _Queries.Add(subrequest.ReferenceId, response.Body?.ToObject<QueryResult<JObject>>() ?? new QueryResult<JObject>());
                                }
                                else if (response.Body?.Type == JTokenType.Array)
                                {
                                    _Results.Add(subrequest.ReferenceId, (JArray)response.Body);
                                }
                                else
                                {
                                    _Results.Add(subrequest.ReferenceId, new JArray { response.Body });
                                }
                            }
                            else
                            {
                                _Errors.Add(subrequest.ReferenceId, DNF.GetErrorResponses(response.Body));
                            }
                        }
                        catch
                        {
                            _Errors.Add(subrequest.ReferenceId, DNF.GetErrorResponses(response.Body));
                        }
                        break;
                    case "object":
                    default:
                        try
                        {
                            if (IsSuccessStatusCode(response.HttpStatusCode))
                            {
                                _Results.Add(subrequest.ReferenceId, response.Body);
                            }
                            else
                            {
                                _Errors.Add(subrequest.ReferenceId, DNF.GetErrorResponses(response.Body));
                            }
                        }
                        catch
                        {
                            _Errors.Add(subrequest.ReferenceId, DNF.GetErrorResponses(response.Body));
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
