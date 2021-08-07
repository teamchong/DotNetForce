using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    //https://developer.salesforce.com/docs/atlas.en-us.214.0.api_rest.meta/api_rest/resources_composite_batch.htm?search_text=connect
    [JetBrains.Annotations.PublicAPI]
    public class BatchRequest : IBatchRequest
    {
        public BatchRequest(bool haltOnError = false)
        {
            Prefix = "";
            HaltOnError = haltOnError;
            BatchRequests = new List<BatchSubRequest>();
        }

        public string Prefix { get; set; }
        public bool HaltOnError { get; set; }
        public IList<BatchSubRequest> BatchRequests { get; set; }

        public BatchSubRequest Limits()
        {
            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = "limits"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest Query(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new BatchSubRequest
            {
                ResponseType = "query",
                Method = "GET",
                Url = $"query?q={Dnf.EscapeDataString(query)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest Explain(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"query?explain={Dnf.EscapeDataString(query)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest QueryAll(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new BatchSubRequest
            {
                ResponseType = "query",
                Method = "GET",
                Url = $"queryAll?q={Dnf.EscapeDataString(query)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest ExplainAll(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"queryAll?explain={Dnf.EscapeDataString(query)}"
            };
            BatchRequests.Add(request);
            return request;
        }


        public BatchSubRequest Search(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));
            if (!query.Contains("FIND")) throw new ArgumentException("query does not contain FIND");
            if (!query.Contains("{") || !query.Contains("}")) throw new ArgumentException("search term must be wrapped in braces");

            var request = new BatchSubRequest
            {
                ResponseType = "query",
                Method = "GET",
                Url = $"search?q={Dnf.EscapeDataString(query)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(BatchRequests);
        }

        #region SObject

        public BatchSubRequest GetObjects()
        {
            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = "sobjects"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest BasicInformation(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest Describe(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest GetDeleted(string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/deleted/?start={sdt}&end={edt}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest GetUpdated(string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", CultureInfo.InvariantCulture));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/updated/?start={sdt}&end={edt}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest Create(string objectName, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var request = new BatchSubRequest
            {
                RichInput = Dnf.UnFlatten(JObject.FromObject(record)),
                Url = $"sobjects/{objectName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest Retrieve(string objectName, string recordId, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{recordId}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                    : $"sobjects/{objectName}/{recordId}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest RetrieveExternal(string objectName, string externalFieldName, string externalId, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                    : $"sobjects/{objectName}/{externalFieldName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest Relationships(string objectName, string recordId, string relationshipFieldName, string[] fields = null)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (string.IsNullOrEmpty(relationshipFieldName)) throw new ArgumentNullException(nameof(relationshipFieldName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{recordId}/{relationshipFieldName}?fields={string.Join(",", fields.Select(Uri.EscapeDataString))}"
                    : $"sobjects/{objectName}/{recordId}/{relationshipFieldName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest Update(string objectName, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var richInput = Dnf.UnFlatten(JObject.FromObject(record));
            return Update(objectName, richInput["Id"]?.ToString(), Dnf.Omit(richInput, "Id"));
        }

        public BatchSubRequest Update(string objectName, string recordId, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var richInput = Dnf.UnFlatten(JObject.FromObject(record));
            var request = new BatchSubRequest
            {
                RichInput = Dnf.Omit(richInput, "Id"),
                Method = "PATCH",
                Url = $"sobjects/{objectName}/{recordId}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest UpsertExternal(string objectName, string externalFieldName, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var richInput = Dnf.UnFlatten(JObject.FromObject(record));
            return UpsertExternal(objectName, externalFieldName, richInput[externalFieldName]?.ToString(), Dnf.Omit(richInput, externalFieldName));
        }

        public BatchSubRequest UpsertExternal(string objectName, string externalFieldName, string externalId, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var richInput = Dnf.UnFlatten(JObject.FromObject(record));
            var request = new BatchSubRequest
            {
                RichInput = Dnf.Omit(richInput, externalFieldName),
                Method = "PATCH",
                Url = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest Delete(string objectName, string recordId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException(nameof(recordId));

            var request = new BatchSubRequest
            {
                Method = "DELETE",
                Url = $"sobjects/{objectName}/{recordId}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest DeleteExternal(string objectName, string externalFieldName, string externalId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException(nameof(externalFieldName));
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException(nameof(externalId));

            var request = new BatchSubRequest
            {
                Method = "DELETE",
                Url = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest NamedLayouts(string objectName, string layoutName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(layoutName)) throw new ArgumentNullException(nameof(layoutName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe/namedLayouts/{layoutName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest ApprovalLayouts(string objectName, string approvalProcessName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe/approvalLayouts/{approvalProcessName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest CompactLayouts(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe/compactLayouts/"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest DescribeLayouts(string objectName = "Global")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe/layouts/"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest PlatformAction(string referenceId)
        {
            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = "sobjects/PlatformAction"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest QuickActions(string objectName, string actionName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/quickActions/{actionName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest QuickActionsDetails(string objectName, string actionName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/quickActions/{actionName}/describe/"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubRequest QuickActionsDefaultValues(string objectName, string actionName, string contextId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrEmpty(contextId)) throw new ArgumentNullException(nameof(contextId));

            var request = new BatchSubRequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/quickActions/{actionName}/defaultValues/{contextId}"
            };
            BatchRequests.Add(request);
            return request;
        }

        #endregion

        #region Connect

        //https://developer.salesforce.com/docs/atlas.en-us.chatterapi.meta/chatterapi/connect_resources_connect.htm
        //to do

        #endregion

        #region Chatter

        //https://developer.salesforce.com/docs/atlas.en-us.chatterapi.meta/chatterapi/intro_what_is_chatter_connect.htm
        //to do

        #endregion

        #region action

        //https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_actions_invocable.htm
        //to do

        #endregion
    }
}
