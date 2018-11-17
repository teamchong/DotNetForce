using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Force;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DotNetForce
{
    public interface IToolingClient
    {
        Task<DescribeGlobalResult<T>> GetObjectsAsync<T>();

        Task<T> BasicInformationAsync<T>(MetadataType metadataType);

        Task<T> DescribeAsync<T>(MetadataType metadataType);

        Task<QueryResult<T>> QueryAsync<T>(string q);

        Task<QueryResult<T>> SearchAsync<T>(string q);

        Task<SaveResponse> CreateAsync(MetadataType metadataType, object record);

        Task<T> RetreiveAsync<T>(MetadataType metadataType, string recordId);

        Task<T> RetreiveAsync<T>(MetadataType metadataType, string recordId, string[] fields);

        Task<SuccessResponse> UpdateAsync(MetadataType metadataType, object record);

        Task<SuccessResponse> UpdateAsync(MetadataType metadataType, string recordId, object record);

        Task<bool> DeleteAsync(MetadataType metadataType, string recordId);


        Task<JToken> CompletionsAsync(string type);

        Task<ExecuteAnonymousResult> ExecuteAnonymousAsync(string anonymousBody);

        Task<JToken> RunTestsAsynchronousAsync(JToken inputObject);

        Task<JToken> RunTestsSynchronousAsync(JToken inputObject);
    }
}
