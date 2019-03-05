using BlazorForce.Models;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Blazor.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorForce
{
    public class AppStorageSetting
    {
        public static async Task<AppStorageSetting> GetAsync()
        {
            AppStorageModel data = null;
            try {
                var storedData = await JsInterop.Current.LocalStorage.GetItemAsync("appStorage");
                data = JsonConvert.DeserializeObject<AppStorageModel>(storedData);
            } catch { }
            return new AppStorageSetting { Data = data ?? new AppStorageModel() };
        }

        public AppStorageModel Data { get; set; }

        private AppStorageSetting() { }

        public async Task SaveChangeAsync()
        {
            await JsInterop.Current.LocalStorage.SetItemAsync("appStorage", Data);
        }
    }
}