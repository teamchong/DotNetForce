using System;
using Microsoft.JSInterop;

namespace BlazorForce
{
    public class JsEventHandler
    {
        private Func<string, dynamic, string> Handler { get; set; }

        public JsEventHandler(Func<string, dynamic, string> handler) => Handler = handler;

        [JSInvokable]
        public string Callback(string type, string ev)
        {
            dynamic dyEv = ev == null ? null : Json.Deserialize<object>(ev);
            return Handler(type, dyEv);
        }
    }
}