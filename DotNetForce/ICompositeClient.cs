using DotNetForce.Common.Models.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace DotNetForce
{
    public interface ICompositeClient
    {
        Task<CompositeResult> PostAsync(ICompositeRequest request);

        Task<BatchResult> BatchAsync(IBatchRequest request);
        
        Task<SaveResponse> CreateTreeAsync<T>(string objectName, IList<T> objectTree);

        IAsyncEnumerable<QueryResult<JObject>> QueryAsync(string query);

        IAsyncEnumerable<QueryResult<T>> QueryAsync<T>(string query);

        IAsyncEnumerable<QueryResult<JObject>> QueryAllAsync(string query);

        IAsyncEnumerable<QueryResult<T>> QueryAllAsync<T>(string query);
        
        IAsyncEnumerable<QueryResult<T>> QueryByLocatorAsync<T>(QueryResult<T>? queryResult);
        IAsyncEnumerable<QueryResult<T>> QueryByLocatorAsync<T>(QueryResult<T>? queryResult, int batchSize);

        IAsyncEnumerable<QueryResult<JObject>> QueryByIdsAsync(IEnumerable<string> source, string templateSoql, string template);
        IAsyncEnumerable<QueryResult<T>> QueryByIdsAsync<T>(IEnumerable<string> source, string templateSoql, string template);

        IAsyncEnumerable<QueryResult<JObject>> QueryByFieldValuesAsync(IEnumerable<string> source, string templateSoql, string template);
        IAsyncEnumerable<QueryResult<T>> QueryByFieldValuesAsync<T>(IEnumerable<string> source, string templateSoql, string template);

        #region Collections

        Task<CompositeResult> CreateAsync<T>(IList<T> records);

        Task<CompositeResult> CreateAsync<T>(IList<T> records, bool all);

        Task<CompositeResult> RetrieveAsync(string objectName, IList<string> ids, params string[] fields);

        Task<CompositeResult> RetrieveExternalAsync(string objectName, string externalFieldName, IList<string> externalIds, params string[] fields);

        Task<CompositeResult> UpdateAsync<T>(IList<T> records);

        Task<CompositeResult> UpdateAsync<T>(IList<T> records, bool allOrNone);

        //Task<CompositeResult> UpsertExternalAsync<T>(string externalFieldName, IList<T> records);

        //Task<CompositeResult> UpsertExternalAsync<T>(string externalFieldName, IList<T> records, bool allOrNone);

        Task<CompositeResult> DeleteAsync(IList<string> ids);

        Task<CompositeResult> DeleteAsync(IList<string> ids, bool allOrNone);

        #endregion
    }
}
