using DotNetForce;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorForce
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
	        System.Net.ServicePointManager.DefaultConnectionLimit = int.MaxValue;
	        System.Net.ServicePointManager.Expect100Continue = true;
	        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder.AllowAnyOrigin()
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials());
            //});
            DNFClient.DefaultApiVersion = "v44.0";
            DNFClient.UseCompression = false;
            DNFClient.Proxy = uri => new Uri(new Uri("https://dotnetforce.herokuapp.com"), uri.PathAndQuery);
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
