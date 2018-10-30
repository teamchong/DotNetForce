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
            var loginProfile = JObject.Parse(File.ReadAllText(jsonFile));
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
            (await (await client.QueryAsync<JObject>($@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%'")).ToEnumerable(client)
.Select(r => r["Id"]?.ToString()).DeleteAsync(client)).ThrowIfError();
            (await (await client.QueryAsync<JObject>($@"
SELECT Id FROM Account WHERE Name LIKE 'UnitTest%'")).ToEnumerable(client)
.Select(r => r["Id"]?.ToString()).DeleteAsync(client)).ThrowIfError();
            (await (await client.QueryAsync<JObject>($@"
SELECT Id FROM Contact WHERE Name LIKE 'UnitTest%'")).ToEnumerable(client)
.Select(r => r["Id"]?.ToString()).DeleteAsync(client)).ThrowIfError();
            (await (await client.QueryAsync<JObject>($@"
SELECT Id FROM Product2 WHERE Source_Product_ID__c LIKE 'UnitTest%'")).ToEnumerable(client)
.Select(r => r["Id"]?.ToString()).DeleteAsync(client)).ThrowIfError();
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
