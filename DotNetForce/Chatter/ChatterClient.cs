using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetForce.Chatter.Models;
using DotNetForce.Common;

namespace DotNetForce.Chatter
{
    internal class ChatterClient : IChatterClient, IDisposable
    {
        private readonly string _itemsOrElements;
        private readonly JsonHttpClient _jsonHttpClient;

        public ChatterClient(string instanceUrl, string accessToken, string apiVersion)
            : this(instanceUrl, accessToken, apiVersion, new HttpClient()) { }

        public ChatterClient(string instanceUrl, string accessToken, string apiVersion, HttpClient httpClient)
        {
            _jsonHttpClient = new JsonHttpClient(instanceUrl, apiVersion, accessToken, httpClient);

            // A change in endpoint for feed item was introduced in v31 of the API.
            _itemsOrElements = float.Parse(_jsonHttpClient.GetApiVersion().Substring(1)) > 30 ? "feed-elements" : "feed-items";
        }

        public ChatterClient(string instanceUrl, string accessToken, string apiVersion, JsonHttpClient jsonHttpClient)
        {
            _jsonHttpClient = jsonHttpClient;

            // A change in endpoint for feed item was introduced in v31 of the API.
            _itemsOrElements = float.Parse(_jsonHttpClient.GetApiVersion().Substring(1)) > 30 ? "feed-elements" : "feed-items";
        }

        public Task<T> FeedsAsync<T>()
        {
            return _jsonHttpClient.HttpGetAsync<T>("chatter/feeds");
        }

        public Task<T> MeAsync<T>()
        {
            return _jsonHttpClient.HttpGetAsync<T>("chatter/users/me");
        }

        public Task<T> PostFeedItemAsync<T>(FeedItemInput feedItemInput, string userId)
        {
            // Feed items not available post v30.0
            if (float.Parse(_jsonHttpClient.GetApiVersion().Substring(1)) > 30.0) return _jsonHttpClient.HttpPostAsync<T>(feedItemInput, "chatter/feed-elements");

            return _jsonHttpClient.HttpPostAsync<T>(feedItemInput, $"chatter/feeds/news/{userId}/{_itemsOrElements}");
        }

        public Task<T> PostFeedItemCommentAsync<T>(FeedItemInput envelope, string feedId)
        {
            if (float.Parse(_jsonHttpClient.GetApiVersion().Substring(1)) > 30.0)
                return _jsonHttpClient.HttpPostAsync<T>(envelope, $"chatter/{_itemsOrElements}/{feedId}/capabilities/comments/items");

            return _jsonHttpClient.HttpPostAsync<T>(envelope, $"chatter/{_itemsOrElements}/{feedId}/comments");
        }

        public Task<T> LikeFeedItemAsync<T>(string feedId)
        {
            if (float.Parse(_jsonHttpClient.GetApiVersion().Substring(1)) > 30.0)
                return _jsonHttpClient.HttpPostAsync<T>(null, $"chatter/{_itemsOrElements}/{feedId}/capabilities/chatter-likes/items");

            return _jsonHttpClient.HttpPostAsync<T>(null, $"chatter/{_itemsOrElements}/{feedId}/likes");
        }

        public Task<T> ShareFeedItemAsync<T>(string feedId, string userId)
        {
            var sharedFeedItem = new SharedFeedItemInput { SubjectId = userId };

            if (float.Parse(_jsonHttpClient.GetApiVersion().Substring(1)) > 30.0)
            {
                sharedFeedItem.OriginalFeedElementId = feedId;
                return _jsonHttpClient.HttpPostAsync<T>(sharedFeedItem, "chatter/feed-elements");
            }

            sharedFeedItem.OriginalFeedItemId = feedId;
            return _jsonHttpClient.HttpPostAsync<T>(sharedFeedItem, $"chatter/feeds/user-profile/{userId}/{_itemsOrElements}");
        }

        public Task<T> GetMyNewsFeedAsync<T>(string query = "")
        {
            var url = $"chatter/feeds/news/me/{_itemsOrElements}";

            if (!string.IsNullOrEmpty(query))
                url += $"?q={query}";

            return _jsonHttpClient.HttpGetAsync<T>(url);
        }

        public Task<T> GetGroupsAsync<T>()
        {
            return _jsonHttpClient.HttpGetAsync<T>("chatter/groups");
        }

        public Task<T> GetGroupFeedAsync<T>(string groupId)
        {
            return _jsonHttpClient.HttpGetAsync<T>($"chatter/feeds/record/{_itemsOrElements}/{groupId}");
        }

        public void Dispose()
        {
            _jsonHttpClient.Dispose();
        }

        public Task<T> PostFeedItemToObjectAsync<T>(ObjectFeedItemInput envelope)
        {
            return _jsonHttpClient.HttpPostAsync<T>(envelope, "chatter/feed-elements/");
        }

        public Task<T> PostFeedItemWithAttachmentAsync<T>(ObjectFeedItemInput envelope, byte[] fileContents, string fileName)
        {
            return _jsonHttpClient.HttpBinaryDataPostAsync<T>("chatter/feed-elements/", envelope, fileContents, "feedElementFileUpload", fileName);
        }

        public Task<T> GetUsersAsync<T>()
        {
            return _jsonHttpClient.HttpGetAsync<T>("chatter/users");
        }

        public Task<T> GetTopicsAsync<T>()
        {
            return _jsonHttpClient.HttpGetAsync<T>("connect/topics");
        }
    }
}
