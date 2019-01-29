using DotNetForce;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
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

   //     [Fact]
   //     public async Task OAuth2Test()
   //     {
			//var client = await DNFClient.OAuthLoginAsync(new OAuthProfile
			//{
			//	LoginUri = new Uri("https://test.salesforce.com"),
			//	ClientId = "3MVG910YPh8zrcR3w3cOaVxURhJtcv8fxvL19jvXzqO_F819av8P2cc9VMnBOKkKTdK.uMAfUGRU_4aYDm5A3",
			//	RedirectUri = "https://ste80.github.io/DotNetForce/oauth2",
			//	Code = ""
			//});

   //         return;
   //     }

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
        public async Task ExplainTest()
        {
            var client = await LoginTask;
            var result = await client.ExplainAsync("SELECT Id, Account.Name, Account.Owner.Name, (SELECT TotalPrice, Product2.Name FROM OpportunityLineItems WHERE TotalPrice > 0) FROM Opportunity WHERE Account.Name LIKE 'Test%'");
            WriteLine(result.ToString());
        }

        [Fact]
        public async Task GetEnumerableTest()
        {
            var expected = 100000;
            var client = await LoginTask;

            await client.LimitsAsync();
            var apiUsed1 = client.ApiUsed;

            var oppty = await client.QueryAsync($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}");
            var oppty2 = JToken.FromObject(oppty).ToObject<QueryResult<JToken>>();
            var oppty3 = (await client.GetEnumerableAsync($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}")).ToList();
            var apiUsed2 = client.ApiUsed;

            var timer1 = Stopwatch.StartNew();
            var opptyList = client.GetEnumerable(oppty).ToArray();
            timer1.Stop();
            var apiUsed3 = client.ApiUsed;
            
            Assert.Equal(expected, oppty3.Count);
            Assert.Equal(expected, opptyList.Length);
            Assert.Equal(expected, opptyList.Select(o => o["Id"]?.ToString())
                .Where(o => o?.StartsWith("006") == true).Count());
            Assert.Equal(expected, opptyList.Select(o => o["Id"]?.ToString())
                .Where(o => o?.StartsWith("006") == true)
                .Distinct().Count());

            var timer2 = Stopwatch.StartNew();
            var opptyList2 = client.GetLazyEnumerable(oppty2).ToArray();
            timer2.Stop();
            var apiUsed4 = client.ApiUsed;

            WriteLine($"time1: {timer1.Elapsed.TotalSeconds}, time2: {timer2.Elapsed.TotalSeconds}.");
            WriteLine($"ApiUsage: {apiUsed1}, {apiUsed2}, {apiUsed3}, {apiUsed4}.");

            Assert.Equal(JToken.FromObject(opptyList.Select(o => o["Id"]?.ToString())).ToString(), JToken.FromObject(opptyList2.Select(o => o["Id"]?.ToString())).ToString());
            WriteLine(JToken.FromObject(opptyList.Select(o => o["Id"]?.ToString())).ToString());
        }

        [Fact]
        public async Task ToLazyEnumerableTest()
        {
            var expected = 100000;
            var client = await LoginTask;

            var apiUsed1 = client.ApiUsed;

            await client.LimitsAsync();
            var apiUsed2 = client.ApiUsed;

            var oppty = await client.QueryAsync($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}");
            var apiUsed3 = client.ApiUsed;

            var result = client.GetLazyEnumerable(oppty).Take(4000).ToArray();
            var apiUsed4 = client.ApiUsed;

            WriteLine($"ApiUsage: {apiUsed1}, {apiUsed2}, {apiUsed3}, {apiUsed4}.");
        }

        [Fact]
        public async Task QueryTest()
        {
            decimal opptyCount = 10000;
            var client = await LoginTask;
            var oppty = await client.QueryAsync(string.Join("", @"
SELECT Id FROM Opportunity LIMIT ", opptyCount));
            var opptyList = client.GetEnumerable(oppty);
            var opptyFullList = JToken.FromObject(opptyList);

            Assert.Equal(opptyCount, opptyFullList.Count());
            Assert.DoesNotContain(opptyFullList, o =>
                o["Id"]?.ToString()?.StartsWith("006") != true);
        }

        [Fact]
        public async Task QueryFailTest()
        {
            await Assert.ThrowsAsync<AggregateException>(async () =>
            {
                var client = await LoginTask;
                var oppty = await client.QueryAsync($@"
SELECT Id, UnkownField FROM Opportunity LIMIT 1");
                var opptyList = client.GetEnumerable(oppty).ToArray();
            });
        }

        [Fact]
        public async Task QueryRelationshipTest()
        {
            var client = await LoginTask;
            var linesList = (await client.GetEnumerableAsync(@"
SELECT Pricebook2Id, COUNT(Id)
FROM Opportunity
GROUP BY Pricebook2Id
ORDER BY COUNT(Id) DESC
LIMIT 10000")).Select(i => i["Pricebook2Id"]?.ToString()).ToList();
            var pricebooks = (await client.GetEnumerableAsync(string.Join("", @"
SELECT Id, (SELECT Id FROM Opportunities), (SELECT Id FROM PricebookEntries)
FROM Pricebook2
WHERE Id IN(", string.Join(",", linesList.Select(l => DNF.SOQLString(l))), @")
ORDER BY Id
LIMIT 10"))).ToList();
            Assert.All(pricebooks, o =>
            {
                var oppSize = (int?)o["Opportunities"]["totalSize"];
                Assert.Equal(oppSize, client.GetEnumerable(o["Opportunities"].ToObject<QueryResult<JToken>>()).Count());
                var peSize = (int?)o["PricebookEntries"]["totalSize"];
                Assert.Equal(peSize, client.GetEnumerable(o["PricebookEntries"].ToObject<QueryResult<JToken>>()).Count());
            });
        }

        [Fact]
        public async Task GetEnumerableByIdsTest()
        {
            var client = await LoginTask;
            var oppIdList = (await client.GetEnumerableAsync(@"
SELECT Id
FROM Opportunity
LIMIT 10000")).Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToList();
            var oppsResult = (await client.GetEnumerableByIdsAsync(oppIdList, @"
SELECT Id
FROM Opportunity
WHERE Id IN(<ids>)
LIMIT 10000", "<ids>")).Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToList();
            Assert.Equal(string.Join(",", oppIdList), string.Join(",", oppsResult));
        }

        [Fact]
        public async Task GetEnumerableByFieldValuesTest()
        {
            var client = await LoginTask;
            var oppIdList = (await client.GetEnumerableAsync(@"
SELECT Id
FROM Opportunity
LIMIT 10000")).Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToList();
            var oppsResult = (await client.GetEnumerableByFieldValuesAsync(oppIdList, @"
SELECT Id
FROM Opportunity
WHERE Id IN(<ids>)
LIMIT 10000", "<ids>")).Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToList();
            Assert.Equal(string.Join(",", oppIdList), string.Join(",", oppsResult));
        }
    }
}
