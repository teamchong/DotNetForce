using DotNetForce.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForce
{
    //https://developer.salesforce.com/docs/atlas.en-us.214.0.api_rest.meta/api_rest/resources_composite_composite.htm?search_text=connect
    public class CompositeRequest : ICompositeRequest
    {
        public CompositeRequest(bool allOrNone = false)
        {
            Prefix = "";
            AllOrNone = allOrNone;
            CompositeRequests = new List<CompositeSubRequest>();
        }

        public string Prefix { get; set; }
        public bool AllOrNone { get; set; }
        public IList<CompositeSubRequest> CompositeRequests { get; set; }

        public CompositeSubRequest Query(string referenceId, string query)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new CompositeSubRequest
            {
                ResponseType = "query",
                Method = "GET",
                ReferenceId = referenceId,
                Url = $@"query?q={Dnf.EscapeDataString(query)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest Explain(string referenceId, string query)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $@"query?explain={Dnf.EscapeDataString(query)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest QueryAll(string referenceId, string query)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new CompositeSubRequest
            {
                ResponseType = "query",
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"queryAll?q={Uri.EscapeDataString(query)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest ExplainAll(string referenceId, string query)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"queryAll?explain={Uri.EscapeDataString(query)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest CreateTree(string referenceId, string objectName, IEnumerable<IAttributedObject> objectTree)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new CompositeSubRequest
            {
                Body = new RecordsObject(objectTree),
                Method = "POST",
                ReferenceId = referenceId,
                Url = $"composite/tree/{objectName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(CompositeRequests);
        }

        #region SObject

        public CompositeSubRequest GetObjects(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = "sobjects"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest BasicInformation(string referenceId, string objectName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest Describe(string referenceId, string objectName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest GetDeleted(string referenceId, string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));

            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/deleted/?start={sdt}&end={edt}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest GetUpdated(string referenceId, string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));

            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/updated/?start={sdt}&end={edt}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest Create(string referenceId, string objectName, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var request = new CompositeSubRequest
            {
                Body = Dnf.UnFlatten(JObject.FromObject(record)),
                Method = "POST",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest Retrieve(string referenceId, string objectName, string recordId, params string[] fields)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = fields.Length > 0
                    ? $"sobjects/{objectName}/{recordId}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                    : $"sobjects/{objectName}/{recordId}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest RetrieveExternal(string referenceId, string objectName, string externalFieldName, string externalId, params string[] fields)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = fields.Length > 0
                    ? $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                    : $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest Relationships(string referenceId, string objectName, string recordId, string relationshipFieldName, string[]? fields = null)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (string.IsNullOrEmpty(relationshipFieldName)) throw new ArgumentNullException(nameof(relationshipFieldName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{recordId}/{relationshipFieldName}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                    : $"sobjects/{objectName}/{recordId}/{relationshipFieldName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest Update(string referenceId, string objectName, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var body = Dnf.UnFlatten(JObject.FromObject(record));
            return Update(referenceId, objectName, body["Id"]?.ToString() ?? string.Empty, Dnf.Omit(body, "Id"));
        }

        public CompositeSubRequest Update(string referenceId, string objectName, string recordId, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var body = Dnf.UnFlatten(JObject.FromObject(record));
            var request = new CompositeSubRequest
            {
                Body = Dnf.Omit(body, "Id"),
                Method = "PATCH",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/{recordId}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest UpsertExternal(string referenceId, string objectName, string externalFieldName, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var body = Dnf.UnFlatten(JObject.FromObject(record));
            return UpsertExternal(referenceId, objectName, externalFieldName, body[externalFieldName]?.ToString() ?? string.Empty, Dnf.Omit(body, externalFieldName));
        }

        public CompositeSubRequest UpsertExternal(string referenceId, string objectName, string externalFieldName, string externalId, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var body = Dnf.UnFlatten(JObject.FromObject(record));
            var request = new CompositeSubRequest
            {
                Body = Dnf.Omit(body, externalFieldName),
                Method = "PATCH",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest Delete(string referenceId, string objectName, string recordId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));

            var request = new CompositeSubRequest
            {
                Method = "DELETE",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/{recordId}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest DeleteExternal(string referenceId, string objectName, string externalFieldName, string externalId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));

            var request = new CompositeSubRequest
            {
                Method = "DELETE",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest NamedLayouts(string referenceId, string objectName, string layoutName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(layoutName)) throw new ArgumentNullException(nameof(layoutName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe/namedLayouts/{layoutName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest ApprovalLayouts(string referenceId, string objectName, string approvalProcessName = "")
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe/approvalLayouts/{approvalProcessName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest CompactLayouts(string referenceId, string objectName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe/compactLayouts/"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest DescribeLayouts(string referenceId, string objectName = "Global")
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe/layouts/"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest PlatformAction(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = "sobjects/PlatformAction"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest QuickActions(string referenceId, string objectName, string actionName = "")
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/quickActions/{actionName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest QuickActionsDetails(string referenceId, string objectName, string actionName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/quickActions/{actionName}/describe/"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubRequest QuickActionsDefaultValues(string referenceId, string objectName, string actionName, string contextId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrEmpty(contextId)) throw new ArgumentNullException(nameof(contextId));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/quickActions/{actionName}/defaultValues/{contextId}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        #endregion

        #region Collections

        public IList<CompositeSubRequest> Create<T>(string referenceId, bool allOrNone, IList<T> records)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (records == null || !records.Any()) throw new ArgumentNullException(nameof(records));

            if (allOrNone && records.Count > 200) throw new ArgumentOutOfRangeException(nameof(records));

            var result = new List<CompositeSubRequest>();

            foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var bodyRecords = JToken.FromObject(chunk.Select(record => record == null ? new JObject() : Dnf.UnFlatten(JObject.FromObject(record))));
                var request = new CompositeSubRequest
                {
                    ResponseType = "collections",
                    Body = new JObject
                    {
                        ["allOrNone"] = allOrNone,
                        ["records"] = bodyRecords
                    },
                    Method = "POST",
                    ReferenceId = $"{referenceId}_{chunkIdx}",
                    Url = "composite/sobjects"
                };
                CompositeRequests.Add(request);
                result.Add(request);
            }
            return result;
        }

        public IList<CompositeSubRequest> Retrieve(string referenceId, string objectName, IList<string> ids, params string[] fields)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            if (fields == null || fields.Length == 0) throw new ArgumentNullException(nameof(fields));

            var result = new List<CompositeSubRequest>();

            foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(ids, 2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var request = new CompositeSubRequest
                {
                    ResponseType = "collections",
                    Body = new JObject
                    {
                        ["ids"] = JToken.FromObject(chunk.Select(id => id)),
                        ["fields"] = JToken.FromObject(fields)
                    },
                    Method = "POST",
                    ReferenceId = $"{referenceId}_{chunkIdx}",
                    Url = $"composite/sobjects/{objectName}"
                };
                CompositeRequests.Add(request);
                result.Add(request);
            }
            return result;
        }

        public IList<CompositeSubRequest> RetrieveExternal(string referenceId, string objectName, string externalFieldName, IList<string> externalIds, params string[] fields)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (externalIds == null || !externalIds.Any()) throw new ArgumentNullException(nameof(externalIds));
            if (fields == null || fields.Length == 0) throw new ArgumentNullException(nameof(fields));

            var result = new List<CompositeSubRequest>();

            foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(externalIds, 2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var request = new CompositeSubRequest
                {
                    ResponseType = "collections",
                    Body = new JObject
                    {
                        ["ids"] = JToken.FromObject(chunk.Select(id => id)),
                        ["fields"] = JToken.FromObject(fields)
                    },
                    Method = "POST",
                    ReferenceId = $"{referenceId}_{chunkIdx}",
                    Url = $"composite/sobjects/{objectName}/{externalFieldName}"
                };
                CompositeRequests.Add(request);
                result.Add(request);
            }
            return result;
        }

        public IList<CompositeSubRequest> Update<T>(string referenceId, bool allOrNone, IList<T> records)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (records == null || !records.Any()) throw new ArgumentNullException(nameof(records));

            if (allOrNone && records.Count > 200) throw new ArgumentOutOfRangeException(nameof(records));
            var result = new List<CompositeSubRequest>();

            foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var bodyRecords = JToken.FromObject(chunk.Select(record => record == null ? new JObject() : Dnf.UnFlatten(JObject.FromObject(record))));
                var request = new CompositeSubRequest
                {
                    ResponseType = "collections",
                    Body = new JObject
                    {
                        ["allOrNone"] = allOrNone,
                        ["records"] = bodyRecords
                    },
                    Method = "PATCH",
                    ReferenceId = $"{referenceId}_{chunkIdx}",
                    Url = "composite/sobjects"
                };
                CompositeRequests.Add(request);
                result.Add(request);
            }
            return result;
        }

        //public IList<CompositeSubRequest> UpsertExternal<T>(string referenceId, string objectName, string externalFieldName, IList<T> records)
        //{
        //    return UpsertExternal(referenceId, false, externalFieldName, records);
        //}

        //public IList<CompositeSubRequest> UpsertExternal<T>(string referenceId, bool allOrNone, string externalFieldName, IList<T> records)
        //{
        //    if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
        //    if (records == null || !records.Any()) throw new ArgumentNullException("records");

        //    var result = new List<CompositeSubRequest>();

        //    if (allOrNone && records.Count() > 200) throw new ArgumentOutOfRangeException("records");

        //    foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(records, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
        //    {
        //        var bodyRecords = JToken.FromObject(chunk.Select(record => Dnf.UnFlatten(JObject.FromObject(record))));
        //        var request = new CompositeSubRequest
        //        {
        //            ResponseType = "collections",
        //            Body = new JObject
        //            {
        //                ["allOrNone"] = allOrNone,
        //                ["records"] = bodyRecords
        //            },
        //            Method = "PATCH",
        //            ReferenceId = $"{referenceId}_{chunkIdx}",
        //            Url = "composite/sobjects"
        //        };
        //        CompositeRequests.Add(request);
        //        result.Add(request);
        //    }
        //    return result;
        //}

        public IList<CompositeSubRequest> Delete(string referenceId, bool allOrNone, params string[] ids)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (ids == null || ids.Length == 0) throw new ArgumentNullException(nameof(ids));

            if (allOrNone && ids.Length > 200) throw new ArgumentOutOfRangeException(nameof(ids));

            var result = new List<CompositeSubRequest>();

            foreach (var (chunk, chunkIdx) in EnumerableChunk.Create(ids, 200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var request = new CompositeSubRequest
                {
                    ResponseType = "collections",
                    Method = "DELETE",
                    ReferenceId = $"{referenceId}_{chunkIdx}",
                    Url = $"composite/sobjects?ids={string.Join(",", chunk.Select(Uri.EscapeDataString))}{(allOrNone ? "&allOrNone=" : "")}"
                };
                CompositeRequests.Add(request);
                result.Add(request);
            }
            return result;
        }

        #endregion
    }
}
