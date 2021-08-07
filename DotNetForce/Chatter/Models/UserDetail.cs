using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Chatter.Models
{
    public class UserDetail
    {
        [JsonProperty(PropertyName = "aboutMe")]
        // ReSharper disable once InconsistentNaming
        public string aboutMe { get; set; }

        [JsonProperty(PropertyName = "address")]
        // ReSharper disable once InconsistentNaming
        public Address address { get; set; }

        [JsonProperty(PropertyName = "chatterActivity")]
        // ReSharper disable once InconsistentNaming
        public ChatterActivity chatterActivity { get; set; }

        [JsonProperty(PropertyName = "chatterInfluence")]
        // ReSharper disable once InconsistentNaming
        public ChatterInfluence chatterInfluence { get; set; }

        [JsonProperty(PropertyName = "companyName")]
        // ReSharper disable once InconsistentNaming
        public string companyName { get; set; }

        [JsonProperty(PropertyName = "email")]
        // ReSharper disable once InconsistentNaming
        public string email { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        // ReSharper disable once InconsistentNaming
        public string firstName { get; set; }

        [JsonProperty(PropertyName = "followersCount")]
        // ReSharper disable once InconsistentNaming
        public int followersCount { get; set; }

        [JsonProperty(PropertyName = "followingCounts")]
        // ReSharper disable once InconsistentNaming
        public FollowingCounts followingCounts { get; set; }

        [JsonProperty(PropertyName = "groupCount")]
        // ReSharper disable once InconsistentNaming
        public int groupCount { get; set; }

        [JsonProperty(PropertyName = "id")]
        // ReSharper disable once InconsistentNaming
        public string id { get; set; }

        [JsonProperty(PropertyName = "isActive")]
        // ReSharper disable once InconsistentNaming
        public bool isActive { get; set; }

        [JsonProperty(PropertyName = "isInThisCommunity")]
        // ReSharper disable once InconsistentNaming
        public bool isInThisCommunity { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        // ReSharper disable once InconsistentNaming
        public string lastName { get; set; }

        [JsonProperty(PropertyName = "managerId")]
        // ReSharper disable once InconsistentNaming
        public string managerId { get; set; }

        [JsonProperty(PropertyName = "managerName")]
        // ReSharper disable once InconsistentNaming
        public string managerName { get; set; }

        [JsonProperty(PropertyName = "motif")]
        // ReSharper disable once InconsistentNaming
        public Motif motif { get; set; }

        [JsonProperty(PropertyName = "mySubscription")]
        // ReSharper disable once InconsistentNaming
        public Reference mySubscription { get; set; }

        [JsonProperty(PropertyName = "name")]
        // ReSharper disable once InconsistentNaming
        public string name { get; set; }

        [JsonProperty(PropertyName = "phoneNumbers")]
        // ReSharper disable once InconsistentNaming
        public IList<PhoneNumber> phoneNumbers { get; set; }

        // ReSharper disable once InconsistentNaming
        [JsonProperty(PropertyName = "photo")]
        public Photo photo { get; set; }

        [JsonProperty(PropertyName = "thanksReceived")]
        // ReSharper disable once InconsistentNaming
        public int? thanksReceived { get; set; }

        [JsonProperty(PropertyName = "title")]
        // ReSharper disable once InconsistentNaming
        public string title { get; set; }

        [JsonProperty(PropertyName = "type")]
        // ReSharper disable once InconsistentNaming
        public string type { get; set; }

        [JsonProperty(PropertyName = "url")]
        // ReSharper disable once InconsistentNaming
        public string url { get; set; }

        [JsonProperty(PropertyName = "username")]
        // ReSharper disable once InconsistentNaming
        public string username { get; set; }

        [JsonProperty(PropertyName = "userType")]
        // ReSharper disable once InconsistentNaming
        public string userType { get; set; }
    }
}
