using DotNetForce.Chatter.Models;
using DotNetForce.Common;
using System;
using System.Net.Http;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForce.Chatter
{
    internal class ChatterClient : IChatterClient, IDisposable
    {
        private readonly string _itemsOrElements;
        private readonly JsonHttpClient _jsonHttpClient;

        public ChatterClient(string? instanceUrl, string? accessToken, string apiVersion)
            : this(instanceUrl, accessToken, apiVersion, new HttpClient()) { }

        public ChatterClient(string? instanceUrl, string? accessToken, string apiVersion, HttpClient httpClient)
        {
            _jsonHttpClient = new JsonHttpClient(instanceUrl, apiVersion, accessToken, httpClient);

            // A change in endpoint for feed item was introduced in v31 of the API.
            _itemsOrElements = float.Parse(_jsonHttpClient.GetApiVersion()[1..]) > 30 ? "feed-elements" : "feed-items";
        }

        public ChatterClient(JsonHttpClient jsonHttpClient)
        {
            _jsonHttpClient = jsonHttpClient;

            // A change in endpoint for feed item was introduced in v31 of the API.
            _itemsOrElements = float.Parse(_jsonHttpClient.GetApiVersion()[1..]) > 30 ? "feed-elements" : "feed-items";
        }

        public Task<T?> FeedsAsync<T>() where T : class
        {
            const string? resourceName = "chatter/feeds";
            return _jsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<T?> MeAsync<T>() where T : class
        {
            const string? resourceName = "chatter/users/me";
            return _jsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<T?> PostFeedItemAsync<T>(FeedItemInput feedItemInput, string userId) where T : class
        {
            // Feed items not available post v30.0
            if (float.Parse(_jsonHttpClient.GetApiVersion()[1..]) > 30.0)
            {
                const string? resourceName = "chatter/feed-elements";
                return _jsonHttpClient.HttpPostAsync<T>(feedItemInput, resourceName);
            }
            else
            {
                var resourceName = $"chatter/feeds/news/{userId}/{_itemsOrElements}";
                return _jsonHttpClient.HttpPostAsync<T>(feedItemInput, resourceName);
            }
        }

        public Task<T?> PostFeedItemCommentAsync<T>(FeedItemInput envelope, string feedId) where T : class
        {
            if (float.Parse(_jsonHttpClient.GetApiVersion()[1..]) > 30.0)
            {
                var resourceName = $"chatter/{_itemsOrElements}/{feedId}/capabilities/comments/items";
                return _jsonHttpClient.HttpPostAsync<T>(envelope, resourceName);
            }
            else
            {
                var resourceName = $"chatter/{_itemsOrElements}/{feedId}/comments";
                return _jsonHttpClient.HttpPostAsync<T>(envelope, resourceName);
            }
        }

        public Task<T?> LikeFeedItemAsync<T>(string feedId) where T : class
        {
            if (float.Parse(_jsonHttpClient.GetApiVersion()[1..]) > 30.0)
            {
                var resourceName = $"chatter/{_itemsOrElements}/{feedId}/capabilities/chatter-likes/items";
                return _jsonHttpClient.HttpPostAsync<T>(null, resourceName);
            }
            else
            {
                var resourceName = $"chatter/{_itemsOrElements}/{feedId}/likes";
                return _jsonHttpClient.HttpPostAsync<T>(null, resourceName);
            }
        }

        public Task<T?> ShareFeedItemAsync<T>(string? feedId, string userId) where T : class
        {
            var sharedFeedItem = new SharedFeedItemInput { SubjectId = userId };

            if (float.Parse(_jsonHttpClient.GetApiVersion()[1..]) > 30.0)
            {
                sharedFeedItem.OriginalFeedElementId = feedId;
                const string? resourceName = "chatter/feed-elements";
                return _jsonHttpClient.HttpPostAsync<T>(sharedFeedItem, resourceName);
            }
            else
            {
                sharedFeedItem.OriginalFeedItemId = feedId;
                var resourceName = $"chatter/feeds/user-profile/{userId}/{_itemsOrElements}";
                return _jsonHttpClient.HttpPostAsync<T>(sharedFeedItem, resourceName);
            }
        }

        public Task<T?> GetMyNewsFeedAsync<T>(string query = "") where T : class
        {
            var resourceName = $"chatter/feeds/news/me/{_itemsOrElements}";

            if (!string.IsNullOrEmpty(query))
                resourceName += $"?q={query}";

            return _jsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<T?> GetGroupsAsync<T>() where T : class
        {
            const string? resourceName = "chatter/groups";
            return _jsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<T?> GetGroupFeedAsync<T>(string groupId) where T : class
        {
            var resourceName = $"chatter/feeds/record/{_itemsOrElements}/{groupId}";
            return _jsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public void Dispose()
        {
            _jsonHttpClient.Dispose();
        }

        public Task<T?> PostFeedItemToObjectAsync<T>(ObjectFeedItemInput envelope) where T : class
        {
            const string? resourceName = "chatter/feed-elements/";
            return _jsonHttpClient.HttpPostAsync<T>(envelope, resourceName);
        }

        public Task<T?> PostFeedItemWithAttachmentAsync<T>(ObjectFeedItemInput envelope, byte[] fileContents, string fileName) where T : class
        {
            const string? resourceName = "chatter/feed-elements/";
            return _jsonHttpClient.HttpBinaryDataPostAsync<T>(resourceName, envelope, fileContents, "feedElementFileUpload", fileName);
        }

        public Task<T?> GetUsersAsync<T>() where T : class
        {
            const string? resourceName = "chatter/users";
            return _jsonHttpClient.HttpGetAsync<T>(resourceName);
        }

        public Task<T?> GetTopicsAsync<T>() where T : class
        {
            const string? resourceName = "connect/topics";
            return _jsonHttpClient.HttpGetAsync<T>(resourceName);
        }
    }
}
