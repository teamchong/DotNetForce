using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;

namespace DotNetForce.Common
{
    [JetBrains.Annotations.PublicAPI]
    public class AuthenticationClient : IAuthenticationClient, IDisposable
    {
        private const string UserAgent = "dotnetforce";
        private const string TokenRequestEndpointUrl = "https://login.salesforce.com/services/oauth2/token";
        private readonly HttpClient _httpClient;

        public AuthenticationClient()
            : this(new HttpClient()) { }

        public AuthenticationClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            ApiVersion = DnfClient.DefaultApiVersion;
        }

        public string InstanceUrl { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Id { get; set; }
        public string ApiVersion { get; set; }

        public Task UsernamePasswordAsync(string clientId, string clientSecret, string username, string password)
        {
            return UsernamePasswordAsync(clientId, clientSecret, username, password, TokenRequestEndpointUrl);
        }

        public async Task UsernamePasswordAsync(string clientId, string clientSecret, string username, string password, string tokenRequestEndpointUrl)
        {
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(tokenRequestEndpointUrl)) throw new ArgumentNullException(nameof(tokenRequestEndpointUrl));
            if (!Uri.IsWellFormedUriString(tokenRequestEndpointUrl, UriKind.Absolute)) throw new FormatException("tokenRequestEndpointUrl");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = DnfClient.Proxy(new Uri(tokenRequestEndpointUrl)),
                Content = content
            };

            request.Headers.UserAgent.ParseAdd(string.Concat(UserAgent, "/", ApiVersion));

            var responseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseMessage.IsSuccessStatusCode)
            {
                try {
                    var authToken = JsonConvert.DeserializeObject<AuthToken>(response);

                    AccessToken = authToken?.AccessToken;
                    InstanceUrl = authToken?.InstanceUrl;
                    Id = authToken?.Id;
                }
                catch (Exception ex) {
                    throw new ForceAuthException(ex.Message, response, responseMessage.StatusCode);
                }
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<AuthErrorResponse>(response);
                throw new ForceAuthException(errorResponse?.Error, errorResponse?.ErrorDescription, responseMessage.StatusCode);
            }
        }

        public Task WebServerAsync(string clientId, string clientSecret, string redirectUri, string code)
        {
            return WebServerAsync(clientId, clientSecret, redirectUri, code, TokenRequestEndpointUrl);
        }

        public async Task WebServerAsync(string clientId, string clientSecret, string redirectUri, string code, string tokenRequestEndpointUrl)
        {
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(nameof(clientId));
            // if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException("clientSecret");
            if (string.IsNullOrEmpty(redirectUri)) throw new ArgumentNullException(nameof(redirectUri));
            if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute)) throw new FormatException("redirectUri");
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));
            if (string.IsNullOrEmpty(tokenRequestEndpointUrl)) throw new ArgumentNullException(nameof(tokenRequestEndpointUrl));
            if (!Uri.IsWellFormedUriString(tokenRequestEndpointUrl, UriKind.Absolute)) throw new FormatException("tokenRequestEndpointUrl");

            var content = string.IsNullOrEmpty(clientSecret)
                ? new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("code", code)
                })
                : new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("code", code)
                });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = DnfClient.Proxy(new Uri(tokenRequestEndpointUrl)),
                Content = content
            };

            request.Headers.UserAgent.ParseAdd(string.Concat(UserAgent, "/", ApiVersion));

            var responseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseMessage.IsSuccessStatusCode)
            {
                var authToken = JsonConvert.DeserializeObject<AuthToken>(response);

                AccessToken = authToken?.AccessToken;
                InstanceUrl = authToken?.InstanceUrl;
                Id = authToken?.Id;
                RefreshToken = authToken?.RefreshToken;
            }
            else
            {
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<AuthErrorResponse>(response);
                    throw new ForceAuthException(errorResponse?.Error, errorResponse?.ErrorDescription);
                }
                catch (Exception ex)
                {
                    throw new ForceAuthException(Error.UnknownException, ex.Message);
                }
            }
        }

        public Task TokenRefreshAsync(string clientId, string refreshToken, string clientSecret = "")
        {
            return TokenRefreshAsync(clientId, refreshToken, clientSecret, TokenRequestEndpointUrl);
        }

        public async Task TokenRefreshAsync(string clientId, string refreshToken, string clientSecret, string tokenRequestEndpointUrl)
        {
            var url = Common.FormatRefreshTokenUrl(
                tokenRequestEndpointUrl,
                clientId,
                refreshToken,
                clientSecret);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = DnfClient.Proxy(new Uri(url))
            };

            request.Headers.UserAgent.ParseAdd(string.Concat(UserAgent, "/", ApiVersion));

            var responseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseMessage.IsSuccessStatusCode)
            {
                var authToken = JsonConvert.DeserializeObject<AuthToken>(response);

                AccessToken = authToken?.AccessToken;
                RefreshToken = refreshToken;
                InstanceUrl = authToken?.InstanceUrl;
                Id = authToken?.Id;
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<AuthErrorResponse>(response);
                throw new ForceException(errorResponse?.Error, errorResponse?.ErrorDescription);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
