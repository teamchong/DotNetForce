using DotNetForce.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DotNetForce
{
    //https://developer.salesforce.com/docs/atlas.en-us.214.0.api_rest.meta/api_rest/resources_composite_composite.htm?search_text=connect
    public class CompositeRequest : ICompositeRequest
    {
        public string Prefix { get; set; }
        public bool AllOrNone { get; set; }
        public List<CompositeSubrequest> CompositeRequests { get; set; }

        public CompositeRequest(bool allOrNone = false)
        {
            Prefix = "";
            AllOrNone = allOrNone;
            CompositeRequests = new List<CompositeSubrequest>();
        }

        #region SObject

        public CompositeSubrequest GetObjects(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = "sobjects"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest BasicInformation(string referenceId, string objectName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest Describe(string referenceId, string objectName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest GetDeleted(string referenceId, string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");

            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", System.Globalization.CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", System.Globalization.CultureInfo.InvariantCulture));

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/deleted/?start={sdt}&end={edt}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest GetUpdated(string referenceId, string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");

            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", System.Globalization.CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", System.Globalization.CultureInfo.InvariantCulture));

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/updated/?start={sdt}&end={edt}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest Create(string referenceId, string objectName, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (record == null) throw new ArgumentNullException("record");

            var request = new CompositeSubrequest
            {
                Body = JObject.FromObject(record).UnFlatten(),
                Method = "POST",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest Retrieve(string referenceId, string objectName, string recordId, params string[] fields)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{recordId}?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}"
                    : $"sobjects/{objectName}/{recordId}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest RetrieveExternal(string referenceId, string objectName, string externalFieldName, string externalId, params string[] fields)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException("externalFieldName");
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException("externalId");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}"
                    : $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest Relationships(string referenceId, string objectName, string recordId, string relationshipFieldName, string[] fields = null)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (string.IsNullOrEmpty(relationshipFieldName)) throw new ArgumentNullException("relationshipFieldName");
            
            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{recordId}/{relationshipFieldName}?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}"
                    : $"sobjects/{objectName}/{recordId}/{relationshipFieldName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest Update(string referenceId, string objectName, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (record == null) throw new ArgumentNullException("record");

            var body = JObject.FromObject(record).UnFlatten();
            return Update(referenceId, objectName, body["Id"]?.ToString(), body.Omit("Id"));
        }

        public CompositeSubrequest Update(string referenceId, string objectName, string recordId, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (record == null) throw new ArgumentNullException("record");

            var body = JObject.FromObject(record).UnFlatten();
            var request = new CompositeSubrequest
            {
                Body = body.Omit("Id"),
                Method = "PATCH",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/{recordId}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest UpsertExternal(string referenceId, string objectName, string externalFieldName, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (record == null) throw new ArgumentNullException("record");

            var body = JObject.FromObject(record).UnFlatten();
            return UpsertExternal(referenceId, objectName, externalFieldName, body[externalFieldName]?.ToString(), body.Omit(externalFieldName));
        }

        public CompositeSubrequest UpsertExternal(string referenceId, string objectName, string externalFieldName, string externalId, object record)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException("externalId");
            if (record == null) throw new ArgumentNullException("record");

            var body = JObject.FromObject(record).UnFlatten();
            var request = new CompositeSubrequest
            {
                Body = body.Omit(externalFieldName),
                Method = "PATCH",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest Delete(string referenceId, string objectName, string recordId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");

            var request = new CompositeSubrequest
            {
                Method = "DELETE",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/{recordId}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest DeleteExternal(string referenceId, string objectName, string externalFieldName, string externalId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException("externalFieldName");
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException("externalId");
            
            var request = new CompositeSubrequest
            {
                Method = "DELETE",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest NamedLayouts(string referenceId, string objectName, string layoutName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(layoutName)) throw new ArgumentNullException("layoutName");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe/namedLayouts/{layoutName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest ApprovalLayouts(string referenceId, string objectName, string approvalProcessName = "")
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe/approvalLayouts/{approvalProcessName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest CompactLayouts(string referenceId, string objectName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe/compactLayouts/"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest DescribeLayouts(string referenceId, string objectName = "Global")
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/describe/layouts/"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest PlatformAction(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/PlatformAction"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest QuickActions(string referenceId, string objectName, string actionName = "")
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/quickActions/{actionName}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest QuickActionsDetails(string referenceId, string objectName, string actionName)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException("actionName");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"sobjects/{objectName}/quickActions/{actionName}/describe/"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest QuickActionsDefaultValues(string referenceId, string objectName, string actionName, string contextId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException("actionName");
            if (string.IsNullOrEmpty(contextId)) throw new ArgumentNullException("contextId");

            var request = new CompositeSubrequest
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
        
        public List<CompositeSubrequest> Create<T>(string referenceId, bool allOrNone, IEnumerable<T> records)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (records == null || !records.Any()) throw new ArgumentNullException("records");
            
            if (allOrNone && records.Count() > 200) throw new ArgumentOutOfRangeException("records");
            
            var result = new List<CompositeSubrequest>();
            
            foreach (var (chunk, chunkIdx) in records.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var bodyRecords = JArray.FromObject(chunk.Select(record => JObject.FromObject(record).UnFlatten()));
                var request = new CompositeSubrequest
                {
                    ResponseType = "collections",
                    Body = new JObject()
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

        public List<CompositeSubrequest> Retrieve(string referenceId, string objectName, IEnumerable<string> ids, params string[] fields)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (ids == null) throw new ArgumentNullException("ids");
            if (fields == null || fields.Length == 0) throw new ArgumentNullException("fields");
            
            var result = new List<CompositeSubrequest>();

            foreach (var (chunk, chunkIdx) in ids.Chunk(2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var request = new CompositeSubrequest
                {
                    ResponseType = "collections",
                    Body = new JObject
                    {
                        ["ids"] = JArray.FromObject(chunk.Select(id => id)),
                        ["fields"] = JArray.FromObject(fields)
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

        public List<CompositeSubrequest> RetrieveExternal(string referenceId, string objectName, string externalFieldName, IEnumerable<string> externalIds, params string[] fields)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException("externalFieldName");
            if (externalIds == null || !externalIds.Any()) throw new ArgumentNullException("externalIds");
            if (fields == null || fields.Length == 0) throw new ArgumentNullException("fields");

            var result = new List<CompositeSubrequest>();

            foreach (var (chunk, chunkIdx) in externalIds.Chunk(2000).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var request = new CompositeSubrequest
                {
                    ResponseType = "collections",
                    Body = new JObject
                    {
                        ["ids"] = JArray.FromObject(chunk.Select(id => id)),
                        ["fields"] = JArray.FromObject(fields)
                    },
                    Method = "POST",
                    ReferenceId = $"{referenceId}_{chunkIdx}",
                    Url = $"composite/sobjects/{externalFieldName}"
                };
                CompositeRequests.Add(request);
                result.Add(request);
            }
            return result;
        }

        public List<CompositeSubrequest> Update<T>(string referenceId, bool allOrNone, IEnumerable<T> records)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (records == null || !records.Any()) throw new ArgumentNullException("records");

            if (allOrNone && records.Count() > 200) throw new ArgumentOutOfRangeException("records");
                        var result = new List<CompositeSubrequest>();

            foreach (var (chunk, chunkIdx) in records.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var bodyRecords = JArray.FromObject(chunk.Select(record => JObject.FromObject(record).UnFlatten()));
                var request = new CompositeSubrequest
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

        //public List<CompositeSubrequest> UpsertExternal<T>(string referenceId, string objectName, string externalFieldName, IEnumerable<T> records)
        //{
        //    return UpsertExternal(referenceId, false, externalFieldName, records);
        //}

        //public List<CompositeSubrequest> UpsertExternal<T>(string referenceId, bool allOrNone, string externalFieldName, IEnumerable<T> records)
        //{
        //    if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
        //    if (records == null || !records.Any()) throw new ArgumentNullException("records");

        //    var result = new List<CompositeSubrequest>();

        //    if (allOrNone && records.Count() > 200) throw new ArgumentOutOfRangeException("records");

        //    foreach (var (chunk, chunkIdx) in records.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
        //    {
        //        var bodyRecords = JArray.FromObject(chunk.Select(record => JObject.FromObject(record).UnFlatten()));
        //        var request = new CompositeSubrequest
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

        public List<CompositeSubrequest> Delete(string referenceId, bool allOrNone, params string[] ids)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (ids == null || ids.Length == 0) throw new ArgumentNullException("ids");

            if (allOrNone && ids.Length > 200) throw new ArgumentOutOfRangeException("ids");
            
            var result = new List<CompositeSubrequest>();

            foreach (var (chunk, chunkIdx) in ids.Chunk(200).Select((chunk, chunkIdx) => (chunk, chunkIdx)))
            {
                var request = new CompositeSubrequest
                {
                    ResponseType = "collections",
                    Method = "DELETE",
                    ReferenceId = $"{referenceId}_{chunkIdx}",
                    Url = $"composite/sobjects?ids={string.Join(",", chunk.Select(id => Uri.EscapeDataString(id)))}{(allOrNone ? "&allOrNone=" : "")}"
                };
                CompositeRequests.Add(request);
                result.Add(request);
            }
            return result;
        }

        #endregion

        public CompositeSubrequest Query(string referenceId, string query)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            var request = new CompositeSubrequest
            {
                ResponseType = "query",
                Method = "GET",
                ReferenceId = referenceId,
                Url = $@"query?q={Uri.EscapeDataString(query)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest QueryAll(string referenceId, string query)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            var request = new CompositeSubrequest
            {
                ResponseType = "query",
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"queryAll?q={Uri.EscapeDataString(query)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public CompositeSubrequest CreateTree(string referenceId, string objectName, IEnumerable<IAttributedObject> objectTree)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new CompositeSubrequest
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
            return JArray.FromObject(CompositeRequests).ToString(0);
        }
    }
}
