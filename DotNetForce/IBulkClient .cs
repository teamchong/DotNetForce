using DotNetForce.Common.Models.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
    public interface IBulkClient
    {
        Task<IList<BatchInfoResult>> RunJobAsync<T>(string objectName, BulkConstants.OperationType operationType, IEnumerable<ISObjectList<T>> recordsLists);

        Task<IList<BatchResultList>> RunJobAndPollAsync<T>(string objectName, BulkConstants.OperationType operationType, IEnumerable<ISObjectList<T>> recordsLists);

        Task<JobInfoResult> CreateJobAsync(string objectName, BulkConstants.OperationType operationType);

        Task<BatchInfoResult> CreateJobBatchAsync<T>(JobInfoResult jobInfo, ISObjectList<T> recordsObject);

        Task<BatchInfoResult> CreateJobBatchAsync<T>(string jobId, ISObjectList<T> recordsObject);

        Task<JobInfoResult> CloseJobAsync(JobInfoResult jobInfo);

        Task<JobInfoResult> CloseJobAsync(string jobId);

        Task<JobInfoResult> PollJobAsync(JobInfoResult jobInfo);

        Task<JobInfoResult> PollJobAsync(string jobId);

        Task<BatchInfoResult> PollBatchAsync(BatchInfoResult batchInfo);

        Task<BatchInfoResult> PollBatchAsync(string batchId, string jobId);

        Task<BatchResultList> GetBatchResultAsync(BatchInfoResult batchInfo);

        Task<BatchResultList> GetBatchResultAsync(string batchId, string jobId);

        Task<Stream> GetBlobAsync(string objectName, string objectId, string fieldName);
    }
}
