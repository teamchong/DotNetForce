using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    //https://developer.salesforce.com/docs/atlas.en-us.api_tooling.meta/api_tooling/tooling_resources_composite_composite.htm
    [JetBrains.Annotations.PublicAPI]
    public class ToolingCompositeRequest : ICompositeRequest
    {
        public ToolingCompositeRequest(bool allOrNone = false)
        {
            Prefix = "tooling/";
            AllOrNone = allOrNone;
            CompositeRequests = new List<CompositeSubRequest>();
        }

        public string Prefix { get; set; }
        public bool AllOrNone { get; set; }
        public IList<CompositeSubRequest> CompositeRequests { get; set; }

        public CompositeSubRequest GetObjects(string referenceId, string objectName)
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

        public CompositeSubRequest Describe(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));

            var request = new CompositeSubRequest
            {
                Method = "GET",
                ReferenceId = referenceId,
                Url = "sobjects/describe"
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

        public CompositeSubRequest Retrieve(string referenceId, string objectName, string recordId)
        {
            return Retrieve(referenceId, objectName, recordId, null);
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
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{recordId}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                    : $"sobjects/{objectName}/{recordId}"
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
            return Update(referenceId, objectName, body["Id"]?.ToString(), Dnf.Omit(body, "Id"));
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
                Url = $"sobjects/{objectName}{recordId}"
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
            return UpsertExternal(referenceId, objectName, externalFieldName, body[externalFieldName]?.ToString(), Dnf.Omit(body, externalFieldName));
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

        public CompositeSubRequest Query(string referenceId, string query)
        {
            if (string.IsNullOrEmpty(referenceId)) throw new ArgumentNullException(nameof(referenceId));
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new CompositeSubRequest
            {
                ResponseType = "query",
                Method = "GET",
                ReferenceId = referenceId,
                Url = $"query?q={Dnf.EscapeDataString(query)}"
            };
            CompositeRequests.Add(request);
            return request;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(CompositeRequests);
        }
    }
}
