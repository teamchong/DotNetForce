using DotNetForce;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit.Abstractions;

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

        protected Uri LoginUri { get; set; }
        protected string ClientId { get; set; }
        protected string ClientSecret { get; set; }
        protected string UserName { get; set; }
        protected string Password { get; set; }

        protected async Task<DnfClient> LoginTask()
        {
            return await DnfClient.LoginAsync(
                LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine).ConfigureAwait(false);
        }

        protected async Task DeleteTestingRecords(DnfClient client)
        {
            Dnf.ThrowIfError(await client.Composite.DeleteAsync(
                await client.GetAsyncEnumerable(@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%'")
                    .Select(r => r["Id"]?.ToString()).ToListAsync().ConfigureAwait(false)
                ).ConfigureAwait(false));
            Dnf.ThrowIfError(await client.Composite.DeleteAsync(
                await client.GetAsyncEnumerable(@"
SELECT Id FROM Account WHERE Name LIKE 'UnitTest%'")
                    .Select(r => r["Id"]?.ToString()).ToListAsync().ConfigureAwait(false)
                ).ConfigureAwait(false));
            Dnf.ThrowIfError(await client.Composite.DeleteAsync(
                await client.GetAsyncEnumerable(@"
SELECT Id FROM Contact WHERE Name LIKE 'UnitTest%'")
                    .Select(r => r["Id"]?.ToString()).ToListAsync().ConfigureAwait(false)
                ).ConfigureAwait(false));
            Dnf.ThrowIfError(await client.Composite.DeleteAsync(
                await client.GetAsyncEnumerable(@"
SELECT Id FROM Product2 WHERE ProductCode LIKE 'UnitTest%'")
                    .Select(r => r["Id"]?.ToString()).ToListAsync().ConfigureAwait(false)
                ).ConfigureAwait(false));
        }

        protected JObject GetTestProduct2()
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
            Debug.WriteLine(msg);
            Output.WriteLine(msg);
        }
    }
}
