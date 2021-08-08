using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace DotNetForce
{
    public interface ILayoutClient
    {

        Task<JObject?> DescribeLayoutAsync(string objectName);

        Task<T?> DescribeLayoutAsync<T>(string objectName) where T : class;

        Task<JObject?> DescribeLayoutAsync(string objectName, string recordTypeId);

        Task<T?> DescribeLayoutAsync<T>(string objectName, string recordTypeId) where T : class;

        Task<JObject?> NamedLayoutsAsync(string objectName, string layoutName);

        Task<T?> NamedLayoutsAsync<T>(string objectName, string layoutName) where T : class;

        Task<JObject?> ApprovalLayoutsAsync(string objectName, string approvalProcessName = "");

        Task<T?> ApprovalLayoutsAsync<T>(string objectName, string approvalProcessName = "") where T : class;

        Task<JObject?> CompactLayoutsAsync(string objectName);

        Task<T?> CompactLayoutsAsync<T>(string objectName) where T : class;

        Task<JObject?> DescribeLayoutsAsync(string objectName = "Global");

        Task<T?> DescribeLayoutsAsync<T>(string objectName = "Global") where T : class;

        Task<JObject?> PlatformActionAsync();

        Task<T?> PlatformActionAsync<T>() where T : class;

        Task<JObject?> QuickActionsAsync(string objectName, string actionName = "");

        Task<T?> QuickActionsAsync<T>(string objectName, string actionName = "") where T : class;

        Task<JObject?> QuickActionsDetailsAsync(string objectName, string actionName);

        Task<T?> QuickActionsDetailsAsync<T>(string objectName, string actionName) where T : class;

        Task<JObject?> QuickActionsDefaultValuesAsync(string objectName, string actionName, string contextId);

        Task<T?> QuickActionsDefaultValuesAsync<T>(string objectName, string actionName, string contextId) where T : class;
    }
}
