using System;
using Microsoft.JSInterop;

namespace BlazorForce
{
    public class SessionHandler
    {
        private Func<string, string> Handler { get; set; }

        public SessionHandler(Func<string, string> handler) => Handler = handler;

        [JSInvokable]
        public string Callback(string key, string value)
        {
            return Handler(value);
        }
    }
}