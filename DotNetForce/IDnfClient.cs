using DotNetForce.Chatter;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

namespace DotNetForce
{
    public interface IDnfClient : IDisposable
    {
        string? InstanceUrl { get; set; }
        string? RefreshToken { get; set; }
        string? AccessToken { get; set; }

        string ApiVersion { get; set; }

        string? Id { get; set; }
        string? IssuedAt { get; set; }
        string? Signature { get; set; }

        Action<string>? Logger { get; set; }

        int? ApiUsed { get; }
        int? ApiLimit { get; }
        int? PerAppApiUsed { get; }
        int? PerAppApiLimit { get; }

        Task<JObject?> LimitsAsync();

        Task<T?> LimitsAsync<T>() where T : class;

        Task<int> DailyApiUsed();

        Task<int> DailyApiLimit();

        Task<IList<JObject>> VersionsAsync();

        Task<IList<T>> VersionsAsync<T>();

        Task<JObject?> ResourcesAsync();

        Task<T?> ResourcesAsync<T>() where T : class;

        Task<JObject?> ResourcesAsync(string apiVersion);

        Task<T?> ResourcesAsync<T>(string apiVersion) where T : class;
        
        #region Client

        JsonHttpClient JsonHttp { get; set; }

        XmlHttpClient XmlHttp { get; set; }

        IChatterClient Chatter { get; set; }

        ICompositeClient Composite { get; set; }

        IToolingClient Tooling { get; set; }

        IBulkClient Bulk { get; set; }

        ILayoutClient Layout { get; set; }

        #endregion

        #region Login

        Task TokenRefreshAsync(Uri loginUri, string clientId, string clientSecret = "");

        #endregion


        #region STANDARD

        Task<JObject?> ExplainAsync(string query);

        Task<T?> ExplainAsync<T>(string query) where T: class;

        IAsyncEnumerable<QueryResult<JObject>> QueryAsync(string query);

        IAsyncEnumerable<QueryResult<T>> QueryAsync<T>(string query);

        IAsyncEnumerable<QueryResult<JObject>> QueryAllAsync(string query);

        IAsyncEnumerable<QueryResult<T>> QueryAllAsync<T>(string query);

        IAsyncEnumerable<QueryResult<T>> QueryByLocatorAsync<T>(QueryResult<T>? queryResult);

        Task<JObject?> QueryByIdAsync(string objectName, string recordId);

        Task<T?> QueryByIdAsync<T>(string objectName, string recordId) where T: class;

        Task<JObject?> ExecuteRestApiAsync(string apiName);

        Task<T?> ExecuteRestApiAsync<T>(string apiName) where  T: class;

        Task<JObject?> ExecuteRestApiAsync(string apiName, object inputObject);

        Task<T?> ExecuteRestApiAsync<T>(string apiName, object inputObject) where T: class;

        Task<SuccessResponse> CreateAsync(string objectName, object record);

        Task<SaveResponse> CreateAsync(string objectName, CreateRequest request);

        Task<SuccessResponse> UpdateAsync(string objectName, string recordId, object record);

        Task<SuccessResponse> UpsertExternalAsync(string objectName, string externalFieldName, string externalId, object record);

        Task<SuccessResponse> UpsertExternalAsync(string objectName, string externalFieldName, string externalId, object record, bool ignoreNull);

        Task<bool> DeleteAsync(string objectName, string recordId);

        Task<bool> DeleteExternalAsync(string objectName, string externalFieldName, string externalId);

        Task<DescribeGlobalResult<JObject>?> GetObjectsAsync();

        Task<DescribeGlobalResult<T>?> GetObjectsAsync<T>();

        Task<JObject?> BasicInformationAsync(string objectName);

        Task<T?> BasicInformationAsync<T>(string objectName) where T: class;

        Task<JObject?> DescribeAsync(string objectName);

        Task<T?> DescribeAsync<T>(string objectName) where T: class;

        Task<JObject?> GetDeleted(string objectName, DateTime startDateTime, DateTime endDateTime);

        Task<T?> GetDeleted<T>(string objectName, DateTime startDateTime, DateTime endDateTime) where T : class;

        Task<JObject?> GetUpdated(string objectName, DateTime startDateTime, DateTime endDateTime);

        Task<T?> GetUpdated<T>(string objectName, DateTime startDateTime, DateTime endDateTime) where T : class;

        Task<JObject?> RecentAsync(int limit = 200);

        Task<T?> RecentAsync<T>(int limit = 200) where T : class;

        Task<IList<JObject>> SearchAsync(string query);

        Task<IList<T>> SearchAsync<T>(string query);

        Task<JObject?> UserInfo();

        Task<T?> UserInfo<T>() where T : class;

        Task<JObject?> UserInfo(string url);

        Task<T?> UserInfo<T>(string url) where T : class;

        #endregion

        #region CRUD

        Task<Stream> RetrieveBlobAsync(string objectName, string recordId, string blobField);

        Task<JObject?> RetrieveExternalAsync(string objectName, string externalFieldName, string externalId, params string[] fields);

        Task<T?> RetrieveExternalAsync<T>(string objectName, string externalFieldName, string externalId, params string[] fields) where T : class;

        Task<Stream> RetrieveRichTextImageAsync(string objectName, string recordId, string fieldName, string contentReferenceId);

        Task<JObject?> RelationshipsAsync(string objectName, string recordId, string relationshipFieldName, string[]? fields = null);

        Task<T?> RelationshipsAsync<T>(string objectName, string recordId, string relationshipFieldName, string[]? fields = null) where T : class;

        #endregion
    }
}
