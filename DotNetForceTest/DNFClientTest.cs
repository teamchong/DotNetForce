using DotNetForce;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
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
            using var client = await LoginTask()
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task LoginFailTest()
        {
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(new Uri("https://www.salesforce.com"), ClientId, ClientSecret, UserName, Password, WriteLine)
                    .ConfigureAwait(false);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(LoginUri, "ClientId", ClientSecret, UserName, Password, WriteLine)
                    .ConfigureAwait(false);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(LoginUri, ClientId, "ClientSecret", UserName, Password, WriteLine)
                    .ConfigureAwait(false);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(LoginUri, ClientId, ClientSecret, "UserName", Password, WriteLine)
                    .ConfigureAwait(false);
            });
            await Assert.ThrowsAsync<ForceAuthException>(async () =>
            {
                using var client = await DnfClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, "Password", WriteLine)
                    .ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task ExplainTest()
        {
            using var client = await LoginTask()
                .ConfigureAwait(false);
            var result = await client.ExplainAsync(
"SELECT Id, Account.Name, Account.Owner.Name, (SELECT TotalPrice, Product2.Name FROM OpportunityLineItems WHERE TotalPrice > 0) FROM Opportunity WHERE Account.Name LIKE 'Test%'")
                .ConfigureAwait(false);
            WriteLine(result?.ToString());
        }

        [Fact]
        public async Task GetEnumerableTest()
        {
            using var client = await LoginTask()
                .ConfigureAwait(false);

            await client.LimitsAsync()
                .ConfigureAwait(false);
            var apiUsed = client.ApiUsed;

            const string soql = @"SELECT Id FROM Opportunity ORDER BY Id";

            var timer1 = Stopwatch.StartNew();
            var oppty1 = await client.QueryAsync(soql).Pull().ToListAsync()
                .ConfigureAwait(false);
            var expected = oppty1.Count;
            var apiUsed1 = client.ApiUsed;
            timer1.Stop();
            var timer2 = Stopwatch.StartNew();
            var oppty2 = await client.QueryAsync(soql).Push().ToList();
            var apiUsed2 = client.ApiUsed;
            timer2.Stop();

            Assert.Equal(expected, oppty1.Count);
            Assert.Equal(expected, oppty2.Count);
            Assert.Equal(expected, oppty1
                .Select(o => o["Id"]?.ToString()).Count(o => o?.StartsWith("006") == true));
            Assert.Equal(expected, oppty1
                .Select(o => o["Id"]?.ToString()).Where(o => o?.StartsWith("006") == true)
                .Distinct().Count());

            WriteLine($"time1: {timer1.Elapsed.TotalSeconds}, time2: {timer2.Elapsed.TotalSeconds}.");
            WriteLine($"ApiUsage: {apiUsed}, {apiUsed1}, {apiUsed2}.");

            Assert.Equal(JToken.FromObject(oppty1.Select(o => o["Id"]?.ToString())).ToString(), JToken.FromObject(oppty2.Select(o => o["Id"]?.ToString())).ToString());
            Assert.Equal(JToken.FromObject(oppty1.Select(o => o["Id"]?.ToString())).ToString(), JToken.FromObject(oppty2.Select(o => o["Id"]?.ToString())).ToString());
            WriteLine(JToken.FromObject(oppty1.Select(o => o["Id"]?.ToString())).ToString());
        }

        [Fact]
        public async Task QueryTest()
        {
            const decimal opptyCount = 100m;
            using var client = await LoginTask()
                .ConfigureAwait(false);
            var opptyList = await client.QueryAsync(string.Join("", @"
SELECT Id FROM Opportunity LIMIT ", opptyCount)).Pull().ToListAsync()
                .ConfigureAwait(false);
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
                using var client = await LoginTask()
                    .ConfigureAwait(false);
                _ = await client.QueryAsync(@"
SELECT Id, UnkownField FROM Opportunity LIMIT 1").Pull().ToListAsync()
                    .ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task QueryRelationshipTest()
        {
            using var client = await LoginTask()
                .ConfigureAwait(false);
            var linesList = await client.QueryAsync(@"
SELECT Pricebook2Id, COUNT(Id)
FROM Opportunity
GROUP BY Pricebook2Id
ORDER BY COUNT(Id) DESC
LIMIT 100").Pull().Select(i => i["Pricebook2Id"]?.ToString()).ToListAsync()
                .ConfigureAwait(false);
            var priceBooks = client.QueryAsync(string.Join("", @"
SELECT Id, (SELECT Id FROM Opportunities), (SELECT Id FROM PricebookEntries)
FROM Pricebook2
WHERE Id IN(", string.Join(",", linesList.Select(Dnf.SoqlString)), @")
ORDER BY Id
LIMIT 10")).Pull();
            
            await foreach (var o in priceBooks)
            {
                var oppSize1 = (int?)o["Opportunities"]?["totalSize"];
                var oppSize2 = await client.Composite.QueryByLocatorAsync(o["Opportunities"]?.ToObject<QueryResult<JToken>>())
                    .Pull().CountAsync()
                    .ConfigureAwait(false);
                Assert.Equal(oppSize1, oppSize2);
                var peSize1 = (int?)o["PricebookEntries"]?["totalSize"];
                var peSize2 = await client.Composite.QueryByLocatorAsync(o["PricebookEntries"]?.ToObject<QueryResult<JToken>>())
                    .Pull().CountAsync()
                    .ConfigureAwait(false);
                Assert.Equal(peSize1, peSize2);
            }
        }

        [Fact]
        public async Task GetEnumerableByIdsTest()
        {
            using var client = await LoginTask()
                .ConfigureAwait(false);
            var oppIdList = await client.QueryAsync(@"
SELECT Id
FROM Opportunity
LIMIT 100").Pull().Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToListAsync()
                .ConfigureAwait(false);
            var oppsResult = await client.Composite.QueryByIdsAsync<JObject>(oppIdList, @"
SELECT Id
FROM Opportunity
WHERE Id IN(<ids>)
LIMIT 100", "<ids>").Pull().Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToListAsync()
                .ConfigureAwait(false);
            Assert.Equal(string.Join(",", oppIdList), string.Join(",", oppsResult));
        }

        [Fact]
        public async Task GetEnumerableByFieldValuesTest()
        {
            using var client = await LoginTask()
                .ConfigureAwait(false);
            var oppIdList = await client.QueryAsync(@"
SELECT Id
FROM Opportunity
LIMIT 100").Pull().Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToListAsync()
                .ConfigureAwait(false);
            var oppsResult = await client.Composite.QueryByFieldValuesAsync(oppIdList, @"
SELECT Id
FROM Opportunity
WHERE Id IN(<ids>)
LIMIT 100", "<ids>").Pull().Select(i => i["Id"]?.ToString()).OrderBy(id => id).ToListAsync()
                .ConfigureAwait(false);
            Assert.Equal(string.Join(",", oppIdList), string.Join(",", oppsResult));
        }
    }
}
