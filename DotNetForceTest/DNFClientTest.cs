using DotNetForce;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DotNetForceTest
{
    public class DnfClientTest : DnfClientTestBase
    {
        public DnfClientTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task LoginTest()
        {
            using var client = await LoginTask();
        }

        [Fact]
        public async Task LoginFailTest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(null, "", "", "", "", WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(new Uri("https://www.salesforce.com"), ClientId, ClientSecret, UserName, Password, WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(LoginUri, "ClientId", ClientSecret, UserName, Password, WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(LoginUri, ClientId, "ClientSecret", UserName, Password, WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(LoginUri, ClientId, ClientSecret, "UserName", Password, WriteLine);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, "Password", WriteLine);
            });
        }

        [Fact]
        public async Task ExplainTest()
        {
            using var client = await LoginTask();
            var result = await client.ExplainAsync(
"SELECT Id, Account.Name, Account.Owner.Name, (SELECT TotalPrice, Product2.Name FROM OpportunityLineItems WHERE TotalPrice > 0) FROM Opportunity WHERE Account.Name LIKE 'Test%'");
            WriteLine(result.ToString());
        }

        [Fact]
        public async Task GetEnumerableTest()
        {
            var expected = 100;
            using var client = await LoginTask();

            await client.LimitsAsync().ConfigureAwait(false);
            var apiUsed1 = client.ApiUsed;

            var oppty = await client.QueryAsync($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}");
            var oppty2 = JToken.FromObject(oppty).ToObject<QueryResult<JToken>>();
            var oppty3 = await client.GetAsyncEnumerable($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}").ToListAsync().ConfigureAwait(false);
            var apiUsed2 = client.ApiUsed;

            var timer1 = Stopwatch.StartNew();
            var opptyList = await client.GetAsyncEnumerable(oppty).ToArrayAsync().ConfigureAwait(false);
            timer1.Stop();
            var apiUsed3 = client.ApiUsed;

            Assert.Equal(expected, oppty3.Count);
            Assert.Equal(expected, opptyList.Length);
            Assert.Equal(expected, opptyList
                .Select(o => o["Id"]?.ToString()).Count(o => o?.StartsWith("006") == true));
            Assert.Equal(expected, opptyList.Select(o => o["Id"]?.ToString())
                .Where(o => o?.StartsWith("006") == true)
                .Distinct().Count());

            var timer2 = Stopwatch.StartNew();
            var opptyList2 = await client.GetAsyncEnumerable(oppty2).ToArrayAsync().ConfigureAwait(false);
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
            var expected = 100;
            using var client = await LoginTask();

            var apiUsed1 = client.ApiUsed;

            await client.LimitsAsync().ConfigureAwait(false);
            var apiUsed2 = client.ApiUsed;

            var oppty = await client.QueryAsync($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}");
            var apiUsed3 = client.ApiUsed;

            _ = await client.GetAsyncEnumerable(oppty).Take(4000).ToArrayAsync().ConfigureAwait(false);
            var apiUsed4 = client.ApiUsed;

            WriteLine($"ApiUsage: {apiUsed1}, {apiUsed2}, {apiUsed3}, {apiUsed4}.");
        }

        [Fact]
        public async Task QueryTest()
        {
            decimal opptyCount = 100;
            using var client = await LoginTask();
            var oppty = await client.QueryAsync(string.Join("", @"
SELECT Id FROM Opportunity LIMIT ", opptyCount));
            var opptyList = await client.GetAsyncEnumerable(oppty).ToListAsync().ConfigureAwait(false);
            var opptyFullList = JToken.FromObject(opptyList);

            Assert.Equal(opptyCount, opptyFullList.Count());
            Assert.DoesNotContain(opptyFullList, o =>
                o["Id"]?.ToString().StartsWith("006") != true);
        }

        [Fact]
        public async Task QueryFailTest()
        {
            await Assert.ThrowsAsync<ForceException>(async () =>
            {
                using var client = await LoginTask();
                var oppty = await client.QueryAsync(@"
SELECT Id, UnkownField FROM Opportunity LIMIT 1");
                _ = await client.GetAsyncEnumerable(oppty).ToArrayAsync().ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task QueryRelationshipTest()
        {
            using var client = await LoginTask();
            var linesList = await client.GetAsyncEnumerable(@"
SELECT Pricebook2Id, COUNT(Id)
FROM Opportunity
GROUP BY Pricebook2Id
ORDER BY COUNT(Id) DESC
LIMIT 100").Select(i => i["Pricebook2Id"]?.ToString()).ToListAsync().ConfigureAwait(false);
            var priceBooks = client.GetAsyncEnumerable(string.Join("", @"
SELECT Id, (SELECT Id FROM Opportunities), (SELECT Id FROM PricebookEntries)
FROM Pricebook2
WHERE Id IN(", string.Join(",", linesList.Select(Dnf.SoqlString)), @")
ORDER BY Id
LIMIT 10"));
            
            await foreach (var o in priceBooks)
            {
                var oppSize1 = (int?)o["Opportunities"]?["totalSize"];
                var oppSize2 = await client.GetAsyncEnumerable(o["Opportunities"]?.ToObject<QueryResult<JToken>>())
                    .CountAsync().ConfigureAwait(false);
                Assert.Equal(oppSize1, oppSize2);
                var peSize1 = (int?)o["PricebookEntries"]?["totalSize"];
                var peSize2 = await client.GetAsyncEnumerable(o["PricebookEntries"]?.ToObject<QueryResult<JToken>>())
                    .CountAsync().ConfigureAwait(false);
                Assert.Equal(peSize1, peSize2);
            }
        }

        [Fact]
        public async Task GetEnumerableByIdsTest()
        {
            using var client = await LoginTask();
            var oppIdList = await client.GetAsyncEnumerable(@"
SELECT Id
FROM Opportunity
LIMIT 100").Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToListAsync().ConfigureAwait(false);
            var oppsResult = await client.GetAsyncEnumerableByIds(oppIdList, @"
SELECT Id
FROM Opportunity
WHERE Id IN(<ids>)
LIMIT 100", "<ids>").Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToListAsync().ConfigureAwait(false);
            Assert.Equal(string.Join(",", oppIdList), string.Join(",", oppsResult));
        }

        [Fact]
        public async Task GetEnumerableByFieldValuesTest()
        {
            using var client = await LoginTask();
            var oppIdList = await client.GetAsyncEnumerable(@"
SELECT Id
FROM Opportunity
LIMIT 100").Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToListAsync().ConfigureAwait(false);
            var oppsResult = await client.GetAsyncEnumerableByFieldValues(oppIdList, @"
SELECT Id
FROM Opportunity
WHERE Id IN(<ids>)
LIMIT 100", "<ids>").Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToListAsync().ConfigureAwait(false);
            Assert.Equal(string.Join(",", oppIdList), string.Join(",", oppsResult));
        }
    }
}
