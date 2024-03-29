﻿using System;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DotNetForce
{
    public class OAuthProfile
    {
        public Uri? LoginUri { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RedirectUri { get; set; }
        public string? Code { get; set; }
    }
}
