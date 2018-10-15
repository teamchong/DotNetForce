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
            var oppty = await client.QueryAsync<JObject>($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}");
            var oppty2 = JToken.FromObject(oppty).ToObject<QueryResult<JObject>>();

            var timer1 = System.Diagnostics.Stopwatch.StartNew();
            var opptyList = oppty.ToEnumerable(client).ToArray();
            timer1.Stop();

            Assert.Equal(expected, opptyList.Length);
            Assert.Equal(expected, opptyList.Select(o => o["Id"]?.ToObject<string>())
                .Where(o => o?.StartsWith("006") == true).Count());
            Assert.Equal(expected, opptyList.Select(o => o["Id"]?.ToObject<string>())
                .Where(o => o?.StartsWith("006") == true)
                .Distinct().Count());

            var timer2 = System.Diagnostics.Stopwatch.StartNew();
            var opptyList2 = oppty2.ToEnumerable(client, false).ToArray();
            timer2.Stop();

            Output.WriteLine($"time1: {timer1.Elapsed.TotalSeconds}, time2: {timer2.Elapsed.TotalSeconds}.");
            Assert.Equal(JArray.FromObject(opptyList.Select(o => o["Id"]?.ToObject<string>())).ToString(), JArray.FromObject(opptyList2.Select(o => o["Id"]?.ToObject<string>())).ToString());
            WriteLine(JArray.FromObject(opptyList.Select(o => o["Id"]?.ToObject<string>())).ToString());
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
                o["Id"]?.ToObject<string>()?.StartsWith("006") != true);
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
LIMIT 1");
            var linesList = lines.ToEnumerable(client)
                .Select(l => l["Pricebook2Id"]?.ToObject<string>()).ToList();
            var pricebooks = await client.QueryAsync<JObject>(string.Join("", @"
SELECT Id, (SELECT Id FROM Opportunities)
FROM Pricebook2
WHERE Id IN(", string.Join(",", linesList.Select(l => DNF.SOQLString(l))), @")
LIMIT 2000"));
            Assert.Contains(pricebooks.Records, o =>
                client.ToEnumerable(o, "Opportunities").Any());
            Assert.DoesNotContain(pricebooks.Records, o =>
                client.ToEnumerable(o, "Opportunities")
                .All(l => l["Id"]?.ToObject<string>()?.StartsWith("006") != true));
        }
    }
}
