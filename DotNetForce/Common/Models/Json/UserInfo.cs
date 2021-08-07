using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetForce.Common.Models.Json
{
    public class UserInfo
    {
        [JsonProperty(PropertyName = "active")]
        public bool Active;

        [JsonProperty(PropertyName = "AddressCity")]
        public string AddressCity;

        [JsonProperty(PropertyName = "AddressCountry")]
        public string AddressCountry;

        [JsonProperty(PropertyName = "AddressState")]
        public string AddressState;

        [JsonProperty(PropertyName = "AddressStreet")]
        public string AddressStreet;

        [JsonProperty(PropertyName = "addr_zip")]
        public string AddressZip;

        [JsonProperty(PropertyName = "asserted_user")]
        public bool AssertedUser;

        [JsonProperty(PropertyName = "custom_attributes")]
        public IDictionary<string, string> CustomAttributes;

        [JsonProperty(PropertyName = "display_name")]
        public string DisplayName;

        [JsonProperty(PropertyName = "email")] public string Email;

        [JsonProperty(PropertyName = "email_verified")]
        public bool EmailVerified;

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName;

        [JsonProperty(PropertyName = "id")] public string Id;

        [JsonProperty(PropertyName = "is_app_installed")]
        public bool IsAppInstalled;

        [JsonProperty(PropertyName = "language")]
        public string Language;

        [JsonProperty(PropertyName = "last_modified_date")]
        public string LastModifiedDate;

        [JsonProperty(PropertyName = "last_name")]
        public string LastName;

        [JsonProperty(PropertyName = "locale")]
        public string Locale;

        [JsonProperty(PropertyName = "mobile_phone")]
        public string MobilePhone;

        [JsonProperty(PropertyName = "mobile_phone_verified")]
        public bool MobilePhoneVerified;

        [JsonProperty(PropertyName = "nick_name")]
        public string NickName;

        [JsonProperty(PropertyName = "organization_id")]
        public string OrganizationId;

        [JsonProperty(PropertyName = "Photos")]
        public IDictionary<string, string> Photos;

        [JsonProperty(PropertyName = "status")]
        public IDictionary<string, string> Status;

        [JsonProperty(PropertyName = "urls")] public IDictionary<string, string> Urls;

        [JsonProperty(PropertyName = "user_id")]
        public string UserId;

        [JsonProperty(PropertyName = "username")]
        public string Username;

        [JsonProperty(PropertyName = "user_type")]
        public string UserType;

        [JsonProperty(PropertyName = "utcOffset")]
        public string UtcOffset;
    }
}
