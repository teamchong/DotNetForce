using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetForce.Common.Models.Json;

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
    public interface ICompositeClient
    {
        Task<CompositeResult> PostAsync(ICompositeRequest request);

        Task<BatchResult> BatchAsync(IBatchRequest request);

        Task<SaveResponse> CreateTreeAsync<T>(string objectName, IList<T> objectTree);

        IAsyncEnumerable<T> GetAsyncEnumerableByQueryResult<T>(QueryResult<T> queryResult);

        IAsyncEnumerable<T> GetAsyncEnumerableByQueryResult<T>(QueryResult<T> queryResult, int batchSize);

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
