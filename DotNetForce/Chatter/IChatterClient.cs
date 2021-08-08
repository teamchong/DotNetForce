using System.Threading.Tasks;
using DotNetForce.Chatter.Models;
// ReSharper disable UnusedMember.Global

namespace DotNetForce.Chatter
{
    public interface IChatterClient
    {
        Task<T?> FeedsAsync<T>() where T : class;
        Task<T?> MeAsync<T>() where T : class;
        Task<T?> PostFeedItemAsync<T>(FeedItemInput feedItemInput, string userId) where T : class;
        Task<T?> PostFeedItemCommentAsync<T>(FeedItemInput envelope, string feedId) where T : class;
        Task<T?> LikeFeedItemAsync<T>(string feedId) where T : class;
        Task<T?> ShareFeedItemAsync<T>(string? feedId, string userId) where T : class;
        Task<T?> GetMyNewsFeedAsync<T>(string query = "") where T : class;
        Task<T?> GetGroupsAsync<T>() where T : class;
        Task<T?> GetGroupFeedAsync<T>(string groupId) where T : class;
    }
}
