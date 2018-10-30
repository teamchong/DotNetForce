using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DotNetForce
{
    //https://developer.salesforce.com/docs/atlas.en-us.214.0.api_rest.meta/api_rest/resources_composite_batch.htm?search_text=connect
    public class BatchRequest : IBatchRequest
    {
        public string Prefix { get; set; }
        public bool HaltOnError { get; set; }
        public List<BatchSubrequest> BatchRequests { get; set; }

        public BatchRequest(bool haltOnError = false)
        {
            Prefix = "";
            HaltOnError = haltOnError;
            BatchRequests = new List<BatchSubrequest>();
        }

        public BatchSubrequest Limits()
        {
            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = "limits"
            };
            BatchRequests.Add(request);
            return request;
        }

        #region SObject

        public BatchSubrequest GetObjects()
        {
            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = "sobjects"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest BasicInformation(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest Describe(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest GetDeleted(string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", System.Globalization.CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", System.Globalization.CultureInfo.InvariantCulture));

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/deleted/?start={sdt}&end={edt}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest GetUpdated(string objectName, DateTime startDateTime, DateTime endDateTime)
        {
            var sdt = Uri.EscapeDataString(startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", System.Globalization.CultureInfo.InvariantCulture));
            var edt = Uri.EscapeDataString(endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss+00:00", System.Globalization.CultureInfo.InvariantCulture));

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/updated/?start={sdt}&end={edt}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest Create(string objectName, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (record == null) throw new ArgumentNullException("record");

            var request = new BatchSubrequest
            {
                RichInput = JObject.FromObject(record).UnFlatten(),
                Url = $"sobjects/{objectName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest Retrieve(string objectName, string recordId, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{recordId}?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}"
                    : $"sobjects/{objectName}/{recordId}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest RetrieveExternal(string objectName, string externalFieldName, string externalId, params string[] fields)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException("externalFieldName");
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException("externalId");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}"
                    : $"sobjects/{objectName}/{externalFieldName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest Relationships(string objectName, string recordId, string relationshipFieldName, string[] fields = null)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (string.IsNullOrEmpty(relationshipFieldName)) throw new ArgumentNullException("relationshipFieldName");
            
            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = fields?.Length > 0
                    ? $"sobjects/{objectName}/{recordId}/{relationshipFieldName}?fields={string.Join(",", fields.Select(field => Uri.EscapeDataString(field)))}"
                    : $"sobjects/{objectName}/{recordId}/{relationshipFieldName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest Update(string objectName, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (record == null) throw new ArgumentNullException("record");

            var richInput = JObject.FromObject(record).UnFlatten();
            return Update(objectName, richInput["Id"]?.ToString(), richInput.Omit("Id"));
        }

        public BatchSubrequest Update(string objectName, string recordId, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");
            if (record == null) throw new ArgumentNullException("record");

            var richInput = JObject.FromObject(record).UnFlatten();
            var request = new BatchSubrequest
            {
                RichInput = richInput.Omit("Id"),
                Method = "PATCH",
                Url = $"sobjects/{objectName}/{recordId}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest UpsertExternal(string objectName, string externalFieldName, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (record == null) throw new ArgumentNullException("record");

            var richInput = JObject.FromObject(record).UnFlatten();
            return UpsertExternal(objectName, externalFieldName, richInput[externalFieldName]?.ToString(), richInput.Omit(externalFieldName));
        }

        public BatchSubrequest UpsertExternal(string objectName, string externalFieldName, string externalId, object record)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException("externalId");
            if (record == null) throw new ArgumentNullException("record");

            var richInput = JObject.FromObject(record).UnFlatten();
            var request = new BatchSubrequest
            {
                RichInput = richInput.Omit(externalFieldName),
                Method = "PATCH",
                Url = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest Delete(string objectName, string recordId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(recordId)) throw new ArgumentNullException("recordId");

            var request = new BatchSubrequest
            {
                Method = "DELETE",
                Url = $"sobjects/{objectName}/{recordId}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest DeleteExternal(string objectName, string externalFieldName, string externalId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(externalFieldName)) throw new ArgumentNullException("externalFieldName");
            if (string.IsNullOrEmpty(externalId)) throw new ArgumentNullException("externalId");
            
            var request = new BatchSubrequest
            {
                Method = "DELETE",
                Url = $"sobjects/{objectName}/{externalFieldName}/{Uri.EscapeDataString(externalId)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest NamedLayouts(string objectName, string layoutName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(layoutName)) throw new ArgumentNullException("layoutName");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe/namedLayouts/{layoutName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest ApprovalLayouts(string objectName, string approvalProcessName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe/approvalLayouts/{approvalProcessName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest CompactLayouts(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe/compactLayouts/"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest DescribeLayouts(string objectName = "Global")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/describe/layouts/"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest PlatformAction(string referenceId)
        {
            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/PlatformAction"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest QuickActions(string objectName, string actionName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/quickActions/{actionName}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest QuickActionsDetails(string objectName, string actionName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException("actionName");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/quickActions/{actionName}/describe/"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest QuickActionsDefaultValues(string objectName, string actionName, string contextId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException("actionName");
            if (string.IsNullOrEmpty(contextId)) throw new ArgumentNullException("contextId");

            var request = new BatchSubrequest
            {
                Method = "GET",
                Url = $"sobjects/{objectName}/quickActions/{actionName}/defaultValues/{contextId}"
            };
            BatchRequests.Add(request);
            return request;
        }
        
        #endregion

        public BatchSubrequest Query(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            var request = new BatchSubrequest
            {
                ResponseType = "query",
                Method = "GET",
                Url = $"query?q={Uri.EscapeDataString(query)}"
            };
            BatchRequests.Add(request);
            return request;
        }

        public BatchSubrequest QueryAll(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            var request = new BatchSubrequest
            {
                ResponseType = "query",
                Method = "GET",
                Url = $"queryAll?q={Uri.EscapeDataString(query)}"
            };
            BatchRequests.Add(request);
            return request;
        }
        
        
        public BatchSubrequest Search(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");
            if (!query.Contains("FIND")) throw new ArgumentException("query does not contain FIND");
            if (!query.Contains("{") || !query.Contains("}")) throw new ArgumentException("search term must be wrapped in braces");

            var request = new BatchSubrequest
            {
                ResponseType = "query",
                Method = "GET",
                Url = $"search?q={Uri.EscapeDataString(query)}"
            };
            BatchRequests.Add(request);
            return request;
        }

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

        public override string ToString()
        {
            return JArray.FromObject(BatchRequests).ToString(0);
        }
    }
}
