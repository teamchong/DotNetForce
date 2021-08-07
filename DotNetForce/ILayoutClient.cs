using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
    public interface ILayoutClient
    {

        Task<JObject> DescribeLayoutAsync(string objectName);

        Task<T> DescribeLayoutAsync<T>(string objectName);

        Task<JObject> DescribeLayoutAsync(string objectName, string recordTypeId);

        Task<T> DescribeLayoutAsync<T>(string objectName, string recordTypeId);

        Task<JObject> NamedLayoutsAsync(string objectName, string layoutName);

        Task<T> NamedLayoutsAsync<T>(string objectName, string layoutName);

        Task<JObject> ApprovalLayoutsAsync(string objectName, string approvalProcessName = "");

        Task<T> ApprovalLayoutsAsync<T>(string objectName, string approvalProcessName = "");

        Task<JObject> CompactLayoutsAsync(string objectName);

        Task<T> CompactLayoutsAsync<T>(string objectName);

        Task<JObject> DescribeLayoutsAsync(string objectName = "Global");

        Task<T> DescribeLayoutsAsync<T>(string objectName = "Global");

        Task<JObject> PlatformActionAsync();

        Task<T> PlatformActionAsync<T>();

        Task<JObject> QuickActionsAsync(string objectName, string actionName = "");

        Task<T> QuickActionsAsync<T>(string objectName, string actionName = "");

        Task<JObject> QuickActionsDetailsAsync(string objectName, string actionName);

        Task<T> QuickActionsDetailsAsync<T>(string objectName, string actionName);

        Task<JObject> QuickActionsDefaultValuesAsync(string objectName, string actionName, string contextId);

        Task<T> QuickActionsDefaultValuesAsync<T>(string objectName, string actionName, string contextId);
    }
}
