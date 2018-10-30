using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace DotNetForce
{
    //https://developer.salesforce.com/docs/atlas.en-us.api_tooling.meta/api_tooling/tooling_resources_composite_composite.htm
    public class ToolingCompositeRequest : ICompositeRequest
    {
        public string Prefix { get; set; }
        public bool AllOrNone { get; set; }
        public List<CompositeSubrequest> CompositeRequests { get; set; }

        public ToolingCompositeRequest(bool allOrNone = false)
        {
            Prefix = "tooling/";
            AllOrNone = allOrNone;
            CompositeRequests = new List<CompositeSubrequest>();
        }

        public CompositeSubrequest GetObjects(string referenceId, string objectName)
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

        public CompositeSubrequest Describe(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");

            var request = new CompositeSubrequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = "sobjects/describe"
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

        public CompositeSubrequest Retrieve(string referenceId, string objectName, string recordId)
        {
            return Retrieve(referenceId, objectName, recordId, null);
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
                Url = $"sobjects/{objectName}{recordId}"
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

        public CompositeSubrequest Query(string referenceId, string query)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException("referenceId");
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            var request = new CompositeSubrequest
            {
                ResponseType = "query",
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"query?q={Uri.EscapeDataString(query)}"
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
