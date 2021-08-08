using DotNetForce.Common.Models.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace DotNetForce
{
    public interface IToolingClient
    {
        Task<DescribeGlobalResult<JObject>?> GetObjectsAsync();
        Task<DescribeGlobalResult<T>?> GetObjectsAsync<T>();

        Task<JObject?> BasicInformationAsync(MetadataType metadataType);
        Task<T?> BasicInformationAsync<T>(MetadataType metadataType) where T : class;

        Task<JObject?> DescribeAsync(MetadataType metadataType);
        Task<T?> DescribeAsync<T>(MetadataType metadataType) where T : class;
        

        IAsyncEnumerable<QueryResult<JObject>> QueryAsync(string query);

        IAsyncEnumerable<QueryResult<T>> QueryAsync<T>(string query);

        IAsyncEnumerable<QueryResult<T>> QueryByLocatorAsync<T>(QueryResult<T>? queryResult);

        Task<JObject?> QueryByIdAsync(string objectName, string recordId);

        Task<T?> QueryByIdAsync<T>(string objectName, string recordId) where T : class;

        IAsyncEnumerable<QueryResult<JObject>> SearchAsync(string q);
        IAsyncEnumerable<QueryResult<T>> SearchAsync<T>(string q);

        Task<SaveResponse> CreateAsync(MetadataType metadataType, object record);

        Task<JObject?> RetrieveAsync(MetadataType metadataType, string recordId, params string[] fields);
        Task<T?> RetrieveAsync<T>(MetadataType metadataType, string recordId, params string[] fields) where T : class;

        Task<SuccessResponse> UpdateAsync(MetadataType metadataType, object record);

        Task<SuccessResponse> UpdateAsync(MetadataType metadataType, string recordId, object record);

        Task<bool> DeleteAsync(MetadataType metadataType, string recordId);


        Task<JToken> CompletionsAsync(string type);

        Task<ExecuteAnonymousResult> ExecuteAnonymousAsync(string anonymousBody);

        Task<JToken> RunTestsAsynchronousAsync(JToken inputObject);

        Task<JToken> RunTestsSynchronousAsync(JToken inputObject);
    }
}
