using LZStringCSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorForce
{
    public class JsInterop
    {
        public static readonly JsInterop Current = new JsInterop();

        public readonly HistoryJsInterop History = new HistoryJsInterop();

        public class HistoryJsInterop
        {
            public async Task BackAsync()
            {
               await JSRuntime.Current.InvokeAsync<object>("DNF.history.back");
            }
            public async Task ForwardAsync()
            {
               await JSRuntime.Current.InvokeAsync<object>("DNF.history.forward");
            }
            public async Task GoAsync(int? delta = null)
            {
               await JSRuntime.Current.InvokeAsync<object>("DNF.history.go", delta);
            }
            public async Task GoAsync(string delta)
            {
               await JSRuntime.Current.InvokeAsync<object>("DNF.history.go", delta);
            }
            public async Task PushStateAsync(object data, string title, string url)
            {
               await JSRuntime.Current.InvokeAsync<object>("DNF.history.pushState", data, title, url);
            }
            public async Task ReplaceStateAsync(object data, string title, string url)
            {
               await JSRuntime.Current.InvokeAsync<object>("DNF.history.replaceState", data, title, url);
            }
        }

        public readonly SessionStorageJsInterop SessionStorage = new SessionStorageJsInterop();

        public class SessionStorageJsInterop
        {
            public async Task<string> GetItemAsync(string key)
            {
               return LZString.DecompressFromUTF16(await JSRuntime.Current.InvokeAsync<string>("DNF.sessionStorage.getItem", key));
            }
            public async Task SetItemAsync(string key, string value)
            {
               await JSRuntime.Current.InvokeAsync<object>("DNF.sessionStorage.setItem", key, value);
            }
        }

        public readonly LocalStorageJsInterop LocalStorage = new LocalStorageJsInterop();

        public class LocalStorageJsInterop
        {
            public async Task<string> GetItemAsync(string key)
            {
               return LZString.DecompressFromUTF16(await JSRuntime.Current.InvokeAsync<string>("DNF.localStorage.getItem", key));
            }
            public async Task SetItemAsync(string key, string value)
            {
                var compressed = LZString.DecompressFromUTF16(value);
                await JSRuntime.Current.InvokeAsync<object>("DNF.localStorage.setItem", key, compressed);
            }
            public async Task SetItemAsync(string key, object value)
            {
                var compressed = LZString.DecompressFromUTF16(JsonConvert.SerializeObject(value));
                await JSRuntime.Current.InvokeAsync<object>("DNF.localStorage.setItem", key, compressed);
            }
        }

        public async Task AddEventListenerAsync(string type) {
            await JSRuntime.Current.InvokeAsync<object>("DNF.addEventListener", type);
        }
    }
}