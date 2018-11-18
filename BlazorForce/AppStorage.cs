using Microsoft.JSInterop;
using Microsoft.AspNetCore.Blazor.Services;
using Newtonsoft.Json;
using LZStringCSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorForce
{
    public class AppStorage
    {
        public static async Task<AppStorage> GetAsync()
        {
            AppStorageModel data = null;
            try {
                var storedData = await JSRuntime.Current.InvokeAsync<string>("DNF.getLocalStorage", "appStorage");
                var uncompressed = LZString.DecompressFromUTF16(storedData);
                data = JsonConvert.DeserializeObject<AppStorageModel>(uncompressed);
            } catch { }
            return new AppStorage { Data = data ?? new AppStorageModel() };
        }

        public AppStorageModel Data { get; set; }

        private AppStorage() { }

        public async Task SaveChangeAsync()
        {
            var compressed = LZString.CompressToUTF16(JsonConvert.SerializeObject(Data));
            await JSRuntime.Current.InvokeAsync<string>("DNF.setLocalStorage", "appStorage", compressed);
        }
    }
}