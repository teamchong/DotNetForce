using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetForce
{
    public class OAuthProfile
    {
        public Uri LoginUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string Code { get; set; }
    }
}
