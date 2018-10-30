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
    public class DNFClientTest : DNFClientTestBase
    {
        public DNFClientTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task LoginTest()
        {
            var client = await LoginTask;
        }

        [Fact]
        public async Task LoginFailTest()
        {
            await Assert.ThrowsAsync<JsonReaderException>(async () =>
            {
                var client = await DNFClient.LoginAsync(new Uri("https://www.salesforce.com"), ClientId, ClientSecret, UserName, Password, WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                var client = await DNFClient.LoginAsync(LoginUri, "ClientId", ClientSecret, UserName, Password, WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                var client = await DNFClient.LoginAsync(LoginUri, ClientId, "ClientSecret", UserName, Password, WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, "UserName", Password, WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, "Password", WriteLine);
            });
        }

        [Fact]
        public async Task ToEnumerableTest()
        {
            var expected = 100000;
            var client = await LoginTask;
            
            await client.LimitsAsync<JObject>();
            var apiUsed1 = client.ApiUsed;

            var oppty = await client.QueryAsync<JObject>($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}");
            var oppty2 = JToken.FromObject(oppty).ToObject<QueryResult<JObject>>();
            var apiUsed2 = client.ApiUsed;

            var timer1 = System.Diagnostics.Stopwatch.StartNew();
            var opptyList = oppty.ToEnumerable(client).ToArray();
            timer1.Stop();
            var apiUsed3 = client.ApiUsed;

            Assert.Equal(expected, opptyList.Length);
            Assert.Equal(expected, opptyList.Select(o => o["Id"]?.ToString())
                .Where(o => o?.StartsWith("006") == true).Count());
            Assert.Equal(expected, opptyList.Select(o => o["Id"]?.ToString())
                .Where(o => o?.StartsWith("006") == true)
                .Distinct().Count());

            var timer2 = System.Diagnostics.Stopwatch.StartNew();
            var opptyList2 = oppty2.ToLazyEnumerable(client).ToArray();
            timer2.Stop();
            var apiUsed4 = client.ApiUsed;

            WriteLine($"time1: {timer1.Elapsed.TotalSeconds}, time2: {timer2.Elapsed.TotalSeconds}.");
            WriteLine($"ApiUsage: {apiUsed1}, {apiUsed2}, {apiUsed3}, {apiUsed4}.");

            Assert.Equal(JArray.FromObject(opptyList.Select(o => o["Id"]?.ToString())).ToString(), JArray.FromObject(opptyList2.Select(o => o["Id"]?.ToString())).ToString());
            WriteLine(JArray.FromObject(opptyList.Select(o => o["Id"]?.ToString())).ToString());
        }

        [Fact]
        public async Task ToLazyEnumerableTest()
        {
            var expected = 100000;
            var client = await LoginTask;
            
            var apiUsed1 = client.ApiUsed;

            await client.LimitsAsync<JObject>();
            var apiUsed2 = client.ApiUsed;
            
            var oppty = await client.QueryAsync<JObject>($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}");
            var apiUsed3 = client.ApiUsed;
            
            var result = oppty.ToLazyEnumerable(client).Take(4000).ToArray();
            var apiUsed4 = client.ApiUsed;
            
            WriteLine($"ApiUsage: {apiUsed1}, {apiUsed2}, {apiUsed3}, {apiUsed4}.");
        }

        [Fact]
        public async Task QueryTest()
        {
            decimal opptyCount = 10000;
            var client = await LoginTask;
            var oppty = await client.QueryAsync<JObject>(string.Join("", @"
SELECT Id FROM Opportunity LIMIT ", opptyCount));
            var opptyList = oppty.ToEnumerable(client);
            var opptyFullList = JArray.FromObject(opptyList);

            Assert.Equal(opptyCount, opptyFullList.Count);
            Assert.DoesNotContain(opptyFullList, o =>
                o["Id"]?.ToString()?.StartsWith("006") != true);
        }

        [Fact]
        public async Task QueryFailTest()
        {
            await Assert.ThrowsAsync<ForceException>(async () =>
            {
                var client = await LoginTask;
                var oppty = await client.QueryAsync<JObject>($@"
SELECT Id, UnkownField FROM Opportunity LIMIT 1");
                var opptyList = oppty.ToEnumerable(client).ToArray();
            });
        }

        [Fact]
        public async Task QueryRelationshipTest()
        {
            var client = await LoginTask;
            var lines = await client.QueryAsync<JObject>(@"
SELECT Pricebook2Id, COUNT(Id)
FROM Opportunity
GROUP BY Pricebook2Id
ORDER BY COUNT(Id) DESC
LIMIT 10000");
            var result = lines.ToEnumerable(client).ToList();
            var linesList = result.Select(l => l["Pricebook2Id"]?.ToString()).ToList();
            var pricebooksResult = await client.QueryAsync<JObject>(string.Join("", @"
SELECT Id, (SELECT Id FROM Opportunities), (SELECT Id FROM PricebookEntries)
FROM Pricebook2
WHERE Id IN(", string.Join(",", linesList.Select(l => DNF.SOQLString(l))), @")
ORDER BY Id
LIMIT 10"));
            var pricebooks = pricebooksResult.ToEnumerable(client).ToArray();
            Assert.All(pricebooks, o => {
                var oppSize = (int?)o["Opportunities"]["totalSize"];
                Assert.Equal(oppSize, client.ToEnumerable(o, "Opportunities").Count());
                var peSize = (int?)o["PricebookEntries"]["totalSize"];
                Assert.Equal(peSize, client.ToEnumerable(o, "PricebookEntries").Count());
            });
        }
    }
}
