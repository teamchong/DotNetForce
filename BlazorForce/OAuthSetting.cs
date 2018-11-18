using Microsoft.JSInterop;
using Microsoft.AspNetCore.Blazor.Services;
using Newtonsoft.Json;
using LZStringCSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorForce
{
    public class OAuthSetting
    {
        public static OAuthSetting Default = new OAuthSetting
        {
            RedirectUri = "https://ste80.github.io/DotNetForce/oauth2",
            ClientId = "3MVG910YPh8zrcR3w3cOaVxURhJtcv8fxvL19jvXzqO_F819av8P2cc9VMnBOKkKTdK.uMAfUGRU_4aYDm5A3"
        };

        public string RedirectUri { get; set; }
        public string ClientId { get; set; }
    }
}