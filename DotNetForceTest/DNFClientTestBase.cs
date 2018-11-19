using DotNetForce;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DotNetForceTest
{
    public class DNFClientTestBase
    {
        protected readonly ITestOutputHelper Output;
        protected Uri LoginUri { get; set; }
        protected string ClientId { get; set; }
        protected string ClientSecret { get; set; }
        protected string UserName { get; set; }
        protected string Password { get; set; }
        protected Task<DNFClient> LoginTask { get; set; }

        public DNFClientTestBase(ITestOutputHelper output)
        {
            Output = output;
	        System.Net.ServicePointManager.DefaultConnectionLimit = int.MaxValue;
	        System.Net.ServicePointManager.Expect100Continue = true;
	        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var dirPath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            while (dirPath.Name != "bin" && dirPath.Parent != null) dirPath = dirPath.Parent;
            dirPath = dirPath.Parent;
            var targetPath = dirPath?.Parent?.Parent?.FullName;
            var jsonFile = targetPath == null ? null : Path.Combine(targetPath, "LoginProfiles.json");
            if (jsonFile != null && !File.Exists(jsonFile))
            {
                var projectPath = dirPath.FullName;
                jsonFile = Path.Combine(projectPath, "LoginProfiles.json");
            }
            var loginProfile = JToken.Parse(File.ReadAllText(jsonFile));
            LoginUri = new Uri(loginProfile["DEV"]["LoginUrl"].ToString());
            ClientId = loginProfile["DEV"]["ClientId"].ToString();
            ClientSecret = loginProfile["DEV"]["ClientSecret"].ToString();
            UserName = loginProfile["DEV"]["UserName"].ToString();
            Password = loginProfile["DEV"]["Password"].ToString();

            LoginTask = Task.Run(async () =>
            {
                return await DNFClient.LoginAsync(
                    LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
            });
        }

        protected async Task DeleteTestingRecords(DNFClient client)
        {
            DNF.ThrowIfError(await client.Composite.DeleteAsync((await client.GetEnumerableAsync($@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%'")).Select(r => r["Id"]?.ToString())));
            DNF.ThrowIfError(await client.Composite.DeleteAsync((await client.GetEnumerableAsync($@"
SELECT Id FROM Account WHERE Name LIKE 'UnitTest%'")).Select(r => r["Id"]?.ToString())));
            DNF.ThrowIfError(await client.Composite.DeleteAsync((await client.GetEnumerableAsync($@"
SELECT Id FROM Contact WHERE Name LIKE 'UnitTest%'")).Select(r => r["Id"]?.ToString())));
            DNF.ThrowIfError(await client.Composite.DeleteAsync((await client.GetEnumerableAsync($@"
SELECT Id FROM Product2 WHERE Source_Product_ID__c LIKE 'UnitTest%'")).Select(r => r["Id"]?.ToString())));
        }

        protected JObject GetTestProduct2()
        {
            var id = Guid.NewGuid();
            return new JObject
            {
                ["attributes"] = new JObject { ["type"] = "Product2" },
                ["Name"] = $"UnitTest{id:N}",
                ["Contract_Type_ID__c"] = "U",
                ["Contract_Type__c"] = "U",
                ["EMS_Rate_ID__c"] = "U",
                ["EMS_Rate_Unique_ID__c"] = 0,
                ["EMS_Sub_Type_ID__c"] = "U",
                ["Venue_ID__c"] = "U",
                ["Zone_ID__c"] = "U",
                ["Source_Product_ID__c"] = $"UnitTest{id:N}",
            };
        }

        protected void WriteLine(string message)
        {
            var msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{message}";
            System.Diagnostics.Debug.WriteLine(msg);
            Output.WriteLine(msg);
        }
    }
}
