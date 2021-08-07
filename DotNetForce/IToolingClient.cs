using System.Threading.Tasks;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json.Linq;

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
    public interface IToolingClient
    {
        Task<DescribeGlobalResult<T>> GetObjectsAsync<T>();

        Task<T> BasicInformationAsync<T>(MetadataType metadataType);

        Task<T> DescribeAsync<T>(MetadataType metadataType);

        Task<QueryResult<T>> QueryAsync<T>(string q);

        Task<QueryResult<T>> SearchAsync<T>(string q);

        Task<SaveResponse> CreateAsync(MetadataType metadataType, object record);

        Task<T> RetrieveAsync<T>(MetadataType metadataType, string recordId);

        Task<T> RetrieveAsync<T>(MetadataType metadataType, string recordId, string[] fields);

        Task<SuccessResponse> UpdateAsync(MetadataType metadataType, object record);

        Task<SuccessResponse> UpdateAsync(MetadataType metadataType, string recordId, object record);

        Task<bool> DeleteAsync(MetadataType metadataType, string recordId);


        Task<JToken> CompletionsAsync(string type);

        Task<ExecuteAnonymousResult> ExecuteAnonymousAsync(string anonymousBody);

        Task<JToken> RunTestsAsynchronousAsync(JToken inputObject);

        Task<JToken> RunTestsSynchronousAsync(JToken inputObject);
    }
}
