using DotNetForce.Chatter;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Models.Xml;
using DotNetForce.Common.Soql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetForce
{
    // ReSharper disable once InconsistentNaming
    [JetBrains.Annotations.PublicAPI]
    public class BulkClient : IBulkClient
    {
        protected readonly XmlHttpClient _xmlHttpClient;
        protected readonly JsonHttpClient _jsonHttpClient;

        public BulkClient(XmlHttpClient xmlHttpClient, JsonHttpClient jsonHttpClient)
        {
            _xmlHttpClient = xmlHttpClient;
            _jsonHttpClient = jsonHttpClient;
        }

        public async Task<IList<BatchInfoResult>> RunJobAsync<T>(string objectName, BulkConstants.OperationType operationType,
            IEnumerable<ISObjectList<T>> recordsLists)
        {
            if (recordsLists == null) throw new ArgumentNullException(nameof(recordsLists));

            var jobInfoResult = await CreateJobAsync(objectName, operationType);
            var batchResults = new List<BatchInfoResult>();
            foreach (var recordList in recordsLists) batchResults.Add(await CreateJobBatchAsync(jobInfoResult, recordList));
            await CloseJobAsync(jobInfoResult);
            return batchResults;
        }

        public async Task<IList<BatchResultList>> RunJobAndPollAsync<T>(string objectName, BulkConstants.OperationType operationType,
            IEnumerable<ISObjectList<T>> recordsLists)
        {
            const float pollingStart = 1000;
            const float pollingIncrease = 2.0f;

            var batchInfoResults = await RunJobAsync(objectName, operationType, recordsLists);

            var currentPoll = pollingStart;
            var finishedBatchInfoResults = new List<BatchInfoResult>();
            while (batchInfoResults.Count > 0)
            {
                var removeList = new List<BatchInfoResult>();
                foreach (var batchInfoResult in batchInfoResults)
                {
                    var batchInfoResultNew = await PollBatchAsync(batchInfoResult);
                    if (batchInfoResultNew.State.Equals(BulkConstants.BatchState.Completed.Value()) ||
                        batchInfoResultNew.State.Equals(BulkConstants.BatchState.Failed.Value()) ||
                        batchInfoResultNew.State.Equals(BulkConstants.BatchState.NotProcessed.Value()))
                    {
                        finishedBatchInfoResults.Add(batchInfoResultNew);
                        removeList.Add(batchInfoResult);
                    }
                }
                foreach (var removeItem in removeList) batchInfoResults.Remove(removeItem);

                await Task.Delay((int)currentPoll);
                currentPoll *= pollingIncrease;
            }


            var batchResults = new List<BatchResultList>();
            foreach (var batchInfoResultComplete in finishedBatchInfoResults) batchResults.Add(await GetBatchResultAsync(batchInfoResultComplete));
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

            return await _xmlHttpClient.HttpPostAsync<JobInfoResult>(jobInfo, "/services/async/{0}/job");
        }

        public async Task<BatchInfoResult> CreateJobBatchAsync<T>(string jobId, ISObjectList<T> recordsObject)
        {
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));
            if (recordsObject == null) throw new ArgumentNullException(nameof(recordsObject));

            return await _xmlHttpClient.HttpPostAsync<BatchInfoResult>(recordsObject, $"/services/async/{{0}}/job/{jobId}/batch")
                .ConfigureAwait(false);
        }

        public async Task<BatchInfoResult> CreateJobBatchAsync<T>(JobInfoResult jobInfo, ISObjectList<T> recordsList)
        {
            if (jobInfo == null) throw new ArgumentNullException(nameof(jobInfo));
            return await CreateJobBatchAsync(jobInfo.Id, recordsList).ConfigureAwait(false);
        }

        public async Task<JobInfoResult> CloseJobAsync(JobInfoResult jobInfo)
        {
            if (jobInfo == null) throw new ArgumentNullException(nameof(jobInfo));
            return await CloseJobAsync(jobInfo.Id);
        }

        public async Task<JobInfoResult> CloseJobAsync(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));

            var state = new JobInfoState { State = "Closed" };
            return await _xmlHttpClient.HttpPostAsync<JobInfoResult>(state, $"/services/async/{{0}}/job/{jobId}")
                .ConfigureAwait(false);
        }

        public Task<JobInfoResult> PollJobAsync(JobInfoResult jobInfo)
        {
            return PollJobAsync(jobInfo?.Id);
        }

        public async Task<JobInfoResult> PollJobAsync(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));

            return await _xmlHttpClient.HttpGetAsync<JobInfoResult>($"/services/async/{{0}}/job/{jobId}")
                .ConfigureAwait(false);
        }

        public async Task<BatchInfoResult> PollBatchAsync(BatchInfoResult batchInfo)
        {
            if (batchInfo == null) throw new ArgumentNullException(nameof(batchInfo));
            return await PollBatchAsync(batchInfo.Id, batchInfo.JobId);
        }

        public async Task<BatchInfoResult> PollBatchAsync(string batchId, string jobId)
        {
            if (string.IsNullOrEmpty(batchId)) throw new ArgumentNullException(nameof(batchId));
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));

            return await _xmlHttpClient.HttpGetAsync<BatchInfoResult>($"/services/async/{{0}}/job/{jobId}/batch/{batchId}")
                .ConfigureAwait(false);
        }

        public async Task<BatchResultList> GetBatchResultAsync(BatchInfoResult batchInfo)
        {
            if (batchInfo == null) throw new ArgumentNullException(nameof(batchInfo));
            return await GetBatchResultAsync(batchInfo.Id, batchInfo.JobId);
        }

        public async Task<BatchResultList> GetBatchResultAsync(string batchId, string jobId)
        {
            if (string.IsNullOrEmpty(batchId)) throw new ArgumentNullException(nameof(batchId));
            if (string.IsNullOrEmpty(jobId)) throw new ArgumentNullException(nameof(jobId));

            return await _xmlHttpClient.HttpGetAsync<BatchResultList>($"/services/async/{{0}}/job/{jobId}/batch/{batchId}/result")
                .ConfigureAwait(false);
        }

        public async Task<Stream> GetBlobAsync(string objectName, string objectId, string fieldName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(objectId)) throw new ArgumentNullException(nameof(objectId));
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            return await _jsonHttpClient.HttpGetBlobAsync($"sobjects/{objectName}/{objectId}/{fieldName}");
        }
    }
}
