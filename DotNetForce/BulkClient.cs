using DotNetForce.Common;
using DotNetForce.Common.Models.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForce
{
    // ReSharper disable once InconsistentNaming
    public class BulkClient : IBulkClient
    {
        protected readonly XmlHttpClient XmlHttpClient;
        protected readonly JsonHttpClient JsonHttpClient;

        public BulkClient(XmlHttpClient xmlHttpClient, JsonHttpClient jsonHttpClient)
        {
            XmlHttpClient = xmlHttpClient;
            JsonHttpClient = jsonHttpClient;
        }

        public async Task<IList<BatchInfoResult>> RunJobAsync<T>(string objectName, BulkConstants.OperationType operationType,
            IEnumerable<ISObjectList<T>> recordsLists)
        {
            if (recordsLists == null) throw new ArgumentNullException(nameof(recordsLists));

            var jobInfoResult = await CreateJobAsync(objectName, operationType)
                .ConfigureAwait(false);
            var batchResults = new List<BatchInfoResult>();
            foreach (var recordList in recordsLists)
                batchResults.Add(await CreateJobBatchAsync(jobInfoResult, recordList)
                    .ConfigureAwait(false));
            await CloseJobAsync(jobInfoResult)
                .ConfigureAwait(false);
            return batchResults;
        }

        public async Task<IList<BatchResultList>> RunJobAndPollAsync<T>(string objectName, BulkConstants.OperationType operationType,
            IEnumerable<ISObjectList<T>> recordsLists)
        {
            const float pollingStart = 1000;
            const float pollingIncrease = 2.0f;

            var batchInfoResults = await RunJobAsync(objectName, operationType, recordsLists)
                .ConfigureAwait(false);

            var currentPoll = pollingStart;
            var finishedBatchInfoResults = new List<BatchInfoResult>();
            while (batchInfoResults.Count > 0)
            {
                var removeList = new List<BatchInfoResult>();
                foreach (var batchInfoResult in batchInfoResults)
                {
                    var batchInfoResultNew = await PollBatchAsync(batchInfoResult)
                        .ConfigureAwait(false);
                    if (batchInfoResultNew.State == null || !batchInfoResultNew.State.Equals(BulkConstants.BatchState.Completed.Value()) &&
                        !batchInfoResultNew.State.Equals(BulkConstants.BatchState.Failed.Value()) &&
                        !batchInfoResultNew.State.Equals(BulkConstants.BatchState.NotProcessed.Value())) continue;
                    finishedBatchInfoResults.Add(batchInfoResultNew);
                    removeList.Add(batchInfoResult);
                }
                foreach (var removeItem in removeList) batchInfoResults.Remove(removeItem);

                await Task.Delay((int)currentPoll)
                    .ConfigureAwait(false);
                currentPoll *= pollingIncrease;
            }


            var batchResults = new List<BatchResultList>();
            foreach (var batchInfoResultComplete in finishedBatchInfoResults)
                batchResults.Add(await GetBatchResultAsync(batchInfoResultComplete)
                    .ConfigureAwait(false));
            return batchResults;
        }

        public async Task<JobInfoResult> CreateJobAsync(string objectName, BulkConstants.OperationType operationType)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));

            var jobInfo = new JobInfo
            {
                ContentType = "XML",
                Object = objectName,
                Operation = operationType.Value()
            };

            const string resourceName = "/services/async/{0}/job";
            return await XmlHttpClient.HttpPostAsync<JobInfoResult>(jobInfo, resourceName)
                .ConfigureAwait(false) ?? new JobInfoResult();
        }

        public async Task<BatchInfoResult> CreateJobBatchAsync<T>(string? jobId, ISObjectList<T> recordsObject)
        {
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));
            if (recordsObject == null) throw new ArgumentNullException(nameof(recordsObject));

            var resourceName = $"/services/async/{{0}}/job/{jobId}/batch";
            return await XmlHttpClient.HttpPostAsync<BatchInfoResult>(recordsObject, resourceName)
                .ConfigureAwait(false) ?? new BatchInfoResult();
        }

        public Task<BatchInfoResult> CreateJobBatchAsync<T>(JobInfoResult jobInfo, ISObjectList<T> recordsList)
        {
            if (jobInfo == null) throw new ArgumentNullException(nameof(jobInfo));
            return CreateJobBatchAsync(jobInfo.Id, recordsList);
        }

        public async Task<JobInfoResult> CloseJobAsync(JobInfoResult jobInfo)
        {
            if (jobInfo == null) throw new ArgumentNullException(nameof(jobInfo));
            return await CloseJobAsync(jobInfo.Id)
                .ConfigureAwait(false) ?? new JobInfoResult();
        }

        public async Task<JobInfoResult> CloseJobAsync(string? jobId)
        {
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));

            var state = new JobInfoState { State = "Closed" };
            var resourceName = $"/services/async/{{0}}/job/{jobId}";
            return await XmlHttpClient.HttpPostAsync<JobInfoResult>(state, resourceName)
                .ConfigureAwait(false) ?? new JobInfoResult();
        }

        public Task<JobInfoResult> PollJobAsync(JobInfoResult? jobInfo)
        {
            return PollJobAsync(jobInfo?.Id);
        }

        public async Task<JobInfoResult> PollJobAsync(string? jobId)
        {
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));

            var resourceName = $"/services/async/{{0}}/job/{jobId}";
            return await XmlHttpClient.HttpGetAsync<JobInfoResult>(resourceName)
                .ConfigureAwait(false) ?? new JobInfoResult();
        }

        public Task<BatchInfoResult> PollBatchAsync(BatchInfoResult batchInfo)
        {
            if (batchInfo == null) throw new ArgumentNullException(nameof(batchInfo));
            return PollBatchAsync(batchInfo.Id ?? string.Empty, batchInfo.JobId ?? string.Empty);
        }

        public async Task<BatchInfoResult> PollBatchAsync(string batchId, string jobId)
        {
            if (string.IsNullOrEmpty(batchId)) throw new ArgumentNullException(nameof(batchId));
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));

            var resourceName = $"/services/async/{{0}}/job/{jobId}/batch/{batchId}";
            return await XmlHttpClient.HttpGetAsync<BatchInfoResult>(resourceName)
                .ConfigureAwait(false) ?? new BatchInfoResult();
        }

        public Task<BatchResultList> GetBatchResultAsync(BatchInfoResult batchInfo)
        {
            if (batchInfo == null) throw new ArgumentNullException(nameof(batchInfo));
            return GetBatchResultAsync(batchInfo.Id ?? string.Empty, batchInfo.JobId ?? string.Empty);
        }

        public async Task<BatchResultList> GetBatchResultAsync(string batchId, string jobId)
        {
            if (string.IsNullOrEmpty(batchId)) throw new ArgumentNullException(nameof(batchId));
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));

            var resourceName = $"/services/async/{{0}}/job/{jobId}/batch/{batchId}/result";
            return await XmlHttpClient.HttpGetAsync<BatchResultList>(resourceName)
                .ConfigureAwait(false) ?? new BatchResultList();
        }

        public Task<Stream> GetBlobAsync(string objectName, string objectId, string fieldName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(objectId)) throw new ArgumentNullException(nameof(objectId));
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            var resourceName = $"sobjects/{objectName}/{objectId}/{fieldName}";
            return JsonHttpClient.HttpGetBlobAsync(resourceName);
        }
    }
}
