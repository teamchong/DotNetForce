using DotNetForce.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace DotNetForce
{
    // ReSharper disable once InconsistentNaming
    [JetBrains.Annotations.PublicAPI]
    public class LayoutClient : ILayoutClient
    {
        protected readonly XmlHttpClient _xmlHttpClient;
        protected readonly JsonHttpClient _jsonHttpClient;

        public LayoutClient(XmlHttpClient xmlHttpClient, JsonHttpClient jsonHttpClient)
        {
            _xmlHttpClient = xmlHttpClient;
            _jsonHttpClient = jsonHttpClient;
        }

        public Task<JObject> DescribeLayoutAsync(string objectName)
        {
            return DescribeLayoutAsync<JObject>(objectName);
        }

        public Task<T> DescribeLayoutAsync<T>(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            return _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/describe/layouts/");
        }

        public Task<JObject> DescribeLayoutAsync(string objectName, string recordTypeId)
        {
            return DescribeLayoutAsync<JObject>(objectName, recordTypeId);
        }

        public Task<T> DescribeLayoutAsync<T>(string objectName, string recordTypeId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(recordTypeId)) throw new ArgumentNullException(nameof(recordTypeId));

            return _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/describe/layouts/{recordTypeId}");
        }

        public Task<JObject> NamedLayoutsAsync(string objectName, string layoutName)
        {
            return NamedLayoutsAsync<JObject>(objectName, layoutName);
        }

        public async Task<T> NamedLayoutsAsync<T>(string objectName, string layoutName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(layoutName)) throw new ArgumentNullException(nameof(layoutName));

            return await _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/describe/namedLayouts/{layoutName}").ConfigureAwait(false);
        }

        public Task<JObject> ApprovalLayoutsAsync(string objectName, string approvalProcessName = "")
        {
            return ApprovalLayoutsAsync<JObject>(objectName, approvalProcessName);
        }

        public async Task<T> ApprovalLayoutsAsync<T>(string objectName, string approvalProcessName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));


            return await _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/describe/approvalLayouts/{approvalProcessName}").ConfigureAwait(false);
        }

        public Task<JObject> CompactLayoutsAsync(string objectName)
        {
            return CompactLayoutsAsync<JObject>(objectName);
        }

        public async Task<T> CompactLayoutsAsync<T>(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            return await _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/describe/compactLayouts/").ConfigureAwait(false);
        }

        public Task<JObject> DescribeLayoutsAsync(string objectName = "Global")
        {
            return DescribeLayoutAsync(objectName);
        }

        public async Task<T> DescribeLayoutsAsync<T>(string objectName = "Global")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            return await _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/describe/layouts/").ConfigureAwait(false);
        }

        public Task<JObject> PlatformActionAsync()
        {
            return PlatformActionAsync<JObject>();
        }

        public async Task<T> PlatformActionAsync<T>()
        {
            return await _jsonHttpClient.HttpGetAsync<T>("sobjects/PlatformAction").ConfigureAwait(false);
        }

        public Task<JObject> QuickActionsAsync(string objectName, string actionName = "")
        {
            return QuickActionsAsync<JObject>(objectName, actionName);
        }

        public async Task<T> QuickActionsAsync<T>(string objectName, string actionName = "")
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            return await _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/quickActions/{actionName}").ConfigureAwait(false);
        }

        public Task<JObject> QuickActionsDetailsAsync(string objectName, string actionName)
        {
            return QuickActionsDetailsAsync<JObject>(objectName, actionName);
        }

        public async Task<T> QuickActionsDetailsAsync<T>(string objectName, string actionName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));

            return await _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/quickActions/{actionName}/describe/").ConfigureAwait(false);
        }

        public Task<JObject> QuickActionsDefaultValuesAsync(string objectName, string actionName, string contextId)
        {
            return QuickActionsDefaultValuesAsync<JObject>(objectName, actionName, contextId);
        }

        public async Task<T> QuickActionsDefaultValuesAsync<T>(string objectName, string actionName, string contextId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(actionName)) throw new ArgumentNullException(nameof(actionName));

            return await _jsonHttpClient.HttpGetAsync<T>($"sobjects/{objectName}/quickActions/{actionName}/defaultValues/{contextId}").ConfigureAwait(false);
        }
    }
}
