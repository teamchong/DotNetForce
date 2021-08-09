using DotNetForce.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
// ReSharper disable NotAccessedField.Global
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForce
{
    // ReSharper disable once InconsistentNaming
    public class LayoutClient : ILayoutClient
    {
        protected readonly XmlHttpClient XmlHttpClient;
        protected readonly JsonHttpClient JsonHttpClient;

        public LayoutClient(XmlHttpClient xmlHttpClient, JsonHttpClient jsonHttpClient)
        {
            XmlHttpClient = xmlHttpClient;
            JsonHttpClient = jsonHttpClient;
        }

        public Task<JObject?> DescribeLayoutAsync(string objectName)
        {
            return DescribeLayoutAsync<JObject>(objectName);
        }

        public Task<T?> DescribeLayoutAsync<T>(string objectName) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var resourceName = $"sobjects/{objectName}/describe/layouts/";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> DescribeLayoutAsync(string objectName, string recordTypeId)
        {
            return DescribeLayoutAsync<JObject>(objectName, recordTypeId);
        }

        public Task<T?> DescribeLayoutAsync<T>(string objectName, string recordTypeId) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordTypeId)) throw new ArgumentNullException(nameof(recordTypeId));

            var resourceName = $"sobjects/{objectName}/describe/layouts/{recordTypeId}";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> NamedLayoutsAsync(string objectName, string layoutName)
        {
            return NamedLayoutsAsync<JObject>(objectName, layoutName);
        }

        public Task<T?> NamedLayoutsAsync<T>(string objectName, string layoutName) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(layoutName)) throw new ArgumentNullException(nameof(layoutName));

            var resourceName = $"sobjects/{objectName}/describe/namedLayouts/{layoutName}";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> ApprovalLayoutsAsync(string objectName, string approvalProcessName = "")
        {
            return ApprovalLayoutsAsync<JObject>(objectName, approvalProcessName);
        }

        public Task<T?> ApprovalLayoutsAsync<T>(string objectName, string approvalProcessName = "") where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));


            var resourceName = $"sobjects/{objectName}/describe/approvalLayouts/{approvalProcessName}";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> CompactLayoutsAsync(string objectName)
        {
            return CompactLayoutsAsync<JObject>(objectName);
        }

        public Task<T?> CompactLayoutsAsync<T>(string objectName) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var resourceName = $"sobjects/{objectName}/describe/compactLayouts/";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> DescribeLayoutsAsync(string objectName = "Global")
        {
            return DescribeLayoutAsync(objectName);
        }

        public Task<T?> DescribeLayoutsAsync<T>(string objectName = "Global") where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var resourceName = $"sobjects/{objectName}/describe/layouts/";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> PlatformActionAsync()
        {
            return PlatformActionAsync<JObject>();
        }

        public Task<T?> PlatformActionAsync<T>() where T : class
        {
            const string? resourceName = "sobjects/PlatformAction";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> QuickActionsAsync(string objectName, string actionName = "")
        {
            return QuickActionsAsync<JObject>(objectName, actionName);
        }

        public Task<T?> QuickActionsAsync<T>(string objectName, string actionName = "") where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var resourceName = $"sobjects/{objectName}/quickActions/{actionName}";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> QuickActionsDetailsAsync(string objectName, string actionName)
        {
            return QuickActionsDetailsAsync<JObject>(objectName, actionName);
        }

        public Task<T?> QuickActionsDetailsAsync<T>(string objectName, string actionName) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));

            var resourceName = $"sobjects/{objectName}/quickActions/{actionName}/describe/";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<JObject?> QuickActionsDefaultValuesAsync(string objectName, string actionName, string contextId)
        {
            return QuickActionsDefaultValuesAsync<JObject>(objectName, actionName, contextId);
        }

        public Task<T?> QuickActionsDefaultValuesAsync<T>(string objectName, string actionName, string contextId) where T : class
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));

            var resourceName = $"sobjects/{objectName}/quickActions/{actionName}/defaultValues/{contextId}";
            return JsonHttpClient.HttpGetAsync<T>(resourceName);
        }
    }
}
