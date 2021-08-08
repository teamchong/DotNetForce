using System;
using DotNetForce.Common.Models.Json;
// ReSharper disable UnusedMember.Global

namespace DotNetForce.Common
{
    public static class Common
    {
        public static Uri FormatUrl(string resourceName, string? instanceUrl, string apiVersion)
        {
            if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException(nameof(resourceName));
            if (string.IsNullOrEmpty(instanceUrl)) throw new ArgumentNullException(nameof(instanceUrl));
            if (string.IsNullOrEmpty(apiVersion)) throw new ArgumentNullException(nameof(apiVersion));

            if (resourceName.StartsWith("/services/data", StringComparison.CurrentCultureIgnoreCase))
                return new Uri(new Uri(instanceUrl), resourceName);

            return resourceName.StartsWith("/services/async", StringComparison.CurrentCultureIgnoreCase) ? new Uri(new Uri(instanceUrl), string.Format(resourceName, apiVersion)) : new Uri(new Uri(instanceUrl), $"/services/data/{apiVersion}/{resourceName}");
        }

        public static Uri FormatCustomUrl(string customApi, string parameters, string instanceUrl)
        {
            if (string.IsNullOrEmpty(customApi)) throw new ArgumentNullException(nameof(customApi));
            if (string.IsNullOrEmpty(parameters)) throw new ArgumentNullException(nameof(parameters));
            if (string.IsNullOrEmpty(instanceUrl)) throw new ArgumentNullException(nameof(instanceUrl));

            return new Uri($"{instanceUrl}/services/apexrest/{customApi}{parameters}");
        }

        public static Uri FormatRestApiUrl(string customApi, string? instanceUrl)
        {
            if (string.IsNullOrEmpty(customApi)) throw new ArgumentNullException(nameof(customApi));
            if (string.IsNullOrEmpty(instanceUrl)) throw new ArgumentNullException(nameof(instanceUrl));

            return new Uri($"{instanceUrl}/services/apexrest/{customApi}");
        }

        public static string FormatAuthUrl(
            string loginUrl,
            ResponseTypes responseType,
            string clientId,
            string redirectUrl,
            DisplayTypes display = DisplayTypes.Page,
            bool immediate = false,
            string state = "",
            string scope = "")
        {
            if (string.IsNullOrEmpty(loginUrl)) throw new ArgumentNullException(nameof(loginUrl));
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(nameof(clientId));
            if (string.IsNullOrEmpty(redirectUrl)) throw new ArgumentNullException(nameof(redirectUrl));

            return 
                $"{loginUrl}?response_type={responseType.ToString().ToLower()}" +
                $"&client_id={clientId}" +
                $"&redirect_uri={redirectUrl}" +
                $"&display={display.ToString().ToLower()}" +
                $"&immediate={immediate}" +
                $"&state={state}" +
                $"&scope={scope}";
        }

        public static string FormatRefreshTokenUrl(
            string tokenRefreshUrl,
            string clientId,
            string? refreshToken,
            string clientSecret = "")
        {
            if (tokenRefreshUrl == null) throw new ArgumentNullException(nameof(tokenRefreshUrl));
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));

            var clientSecretQuerystring = string.Empty;
            if (!string.IsNullOrEmpty(clientSecret)) clientSecretQuerystring = $"&client_secret={clientSecret}";

            return 
                $"{tokenRefreshUrl}?grant_type=refresh_token" +
                $"&client_id={clientId}{clientSecretQuerystring}" +
                $"&refresh_token={refreshToken}";
        }
    }
}
