using DotNetForce;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit.Abstractions;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace DotNetForceTest
{
    public class DnfClientTestBase
    {
        protected readonly ITestOutputHelper Output;

        // dotnet user-secrets set "DotNetForce:LoginUrl" "https://test.salesforce.com"
        // dotnet user-secrets set "DotNetForce:ClientId" ""
        // dotnet user-secrets set "DotNetForce:ClientSecret" ""
        // dotnet user-secrets set "DotNetForce:UserName" ""
        // dotnet user-secrets set "DotNetForce:Password" ""
        public DnfClientTestBase(ITestOutputHelper output)
        {
            Output = output;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var config = new ConfigurationBuilder()
                .AddUserSecrets<DnfClientTestBase>()
                .Build();
            LoginUri = new Uri(config["DotNetForce:LoginUrl"] ?? "");
            ClientId = config["DotNetForce:ClientId"] ?? "";
            ClientSecret = config["DotNetForce:ClientSecret"] ?? "";
            UserName = config["DotNetForce:UserName"] ?? "";
            Password = config["DotNetForce:Password"] ?? "";
        }

        protected Uri LoginUri { get; }
        protected string ClientId { get; }
        protected string ClientSecret { get; }
        protected string UserName { get; }
        protected string Password { get; }

        protected Task<DnfClient> LoginTask()
        {
            var client = DnfClient.LoginAsync(
                LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
            return client;
        }

        protected static async Task DeleteTestingRecords(DnfClient client)
        {
            (await client.Composite.DeleteAsync(
                await client.QueryAsync(@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%'").Pull()
                    .Select(r => r["Id"]?.ToString()).ToListAsync()
                    .ConfigureAwait(false))
                .ConfigureAwait(false)).Assert();
            (await client.Composite.DeleteAsync(
                await client.QueryAsync(@"
SELECT Id FROM Account WHERE Name LIKE 'UnitTest%'").Pull()
                    .Select(r => r["Id"]?.ToString()).ToListAsync()
                    .ConfigureAwait(false))
                .ConfigureAwait(false)).Assert();
            (await client.Composite.DeleteAsync(
                await client.QueryAsync(@"
SELECT Id FROM Contact WHERE Name LIKE 'UnitTest%'").Pull()
                    .Select(r => r["Id"]?.ToString()).ToListAsync()
                    .ConfigureAwait(false))
                .ConfigureAwait(false)).Assert();
            (await client.Composite.DeleteAsync(
                await client.QueryAsync(@"
SELECT Id FROM Product2 WHERE ProductCode LIKE 'UnitTest%'").Pull()
                    .Select(r => r["Id"]?.ToString()).ToListAsync()
                    .ConfigureAwait(false))
                .ConfigureAwait(false)).Assert();
        }

        protected static JObject GetTestProduct2()
        {
            var id = Guid.NewGuid();
            return new JObject
            {
                ["attributes"] = new JObject { ["type"] = "Product2" },
                ["Name"] = $"UnitTest{id:N}",
                ["ProductCode"] = $"UnitTest{id:N}",
                ["CurrencyIsoCode"] = "USD"
            };
        }

        protected void WriteLine(string message)
        {
            var msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{message}";
#if DEBUG
            Debug.WriteLine(msg);
#endif
            Output.WriteLine(msg);
        }
    }
}
