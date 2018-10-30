using DotNetForce.Chatter;
using DotNetForce.Chatter.Models;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Models.Xml;
using DotNetForce.Force;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DotNetForce
{
    public interface ICompositeClient
    {
        #region Collections
        
        Task<CompositeResult> CreateAsync<T>(IEnumerable<T> records);
        
        Task<CompositeResult> CreateAsync<T>(IEnumerable<T> records, bool all);

        Task<CompositeResult> RetrieveAsync(string objectName, IEnumerable<string> ids, params string[] fields);

        Task<CompositeResult> RetrieveExternalAsync(string objectName, string externalFieldName, IEnumerable<string> externalIds, params string[] fields);

        Task<CompositeResult> UpdateAsync<T>(IEnumerable<T> records);

        Task<CompositeResult> UpdateAsync<T>(IEnumerable<T> records, bool allOrNone);

        //Task<CompositeResult> UpsertExternalAsync<T>(string externalFieldName, IEnumerable<T> records);

        //Task<CompositeResult> UpsertExternalAsync<T>(string externalFieldName, IEnumerable<T> records, bool allOrNone);

        Task<CompositeResult> DeleteAsync(IEnumerable<string> ids);

        Task<CompositeResult> DeleteAsync(IEnumerable<string> ids, bool allOrNone);

        #endregion

        Task<CompositeResult> PostAsync(ICompositeRequest request);

        Task<BatchResult> BatchAsync(IBatchRequest request);

        Task<SaveResponse> CreateTreeAsync<T>(string objectName, IEnumerable<T> objectTree);
    }
}
