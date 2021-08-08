using DotNetForce;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DotNetForceTest
{
    public class DnfClientCrudTest : DnfClientTestBase
    {
        public DnfClientCrudTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task CreateTest()
        {
            var client = await LoginTask()
                .ConfigureAwait(false);

            await DeleteTestingRecords(client)
                .ConfigureAwait(false);

            try
            {
                var result1 = await client
                    .CreateAsync("Case", new JObject
                    {
                        ["Subject"] = "UnitTest0"
                    })
                    .ConfigureAwait(false);
                result1.Assert();

                var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}"
                }).ToArray();
                var result = await client.Composite.CreateAsync(caseList, true)
                    .ConfigureAwait(false);
                result.Assert();
                Assert.NotEmpty(result.SuccessResponses());

                const int expected = 200 * 26;
                var existingCase = await client.QueryAsync($@"
SELECT Name FROM Opportunity ORDER BY Id LIMIT {expected}").Pull().ToListAsync()
                    .ConfigureAwait(false);

                var newCase = await client.Composite.CreateAsync(existingCase
                    .Select(c => new AttributedObject("Case") { ["Subject"] = $"UnitTest{c["Name"]}" }).ToList())
                    .ConfigureAwait(false);
                newCase.Assert();
                Assert.NotEmpty(newCase.SuccessResponses());

                var obj = new AttributedObject("Account", $"acc{Guid.NewGuid():N}")
                {
                    ["Name"] = $"UnitTest Account {Guid.NewGuid():N}",
                    ["BillingCountry"] = "Hong Kong",
                    ["Contacts"] = new RecordsObject(Enumerable.Range(1, 2).Select(j =>
                        new AttributedObject("Contact", $"cont{Guid.NewGuid():N}")
                            {
                                ["FirstName"] = $"UnitTest Contact A{(char)('a'+j)}",
                                ["LastName"] = $"UnitTest Contact A{(char)('a'+j)}",
                                ["MailingCountry"] = "Hong Kong"
                            }
                    ))
                };
                var createTreeResult = await client
                    .CreateAsync("Account", new RecordsObject(new[] { obj }).ToCreateRequest())
                    .ConfigureAwait(false);
                createTreeResult.Assert();
                Assert.NotEmpty(createTreeResult.Results ?? new List<SaveResult>());

                var accounts =
                    Enumerable.Range(1, 6).Select(i => new AttributedObject("Account", $"acc{i}")
                    {
                        ["Name"] = $"UnitTest Account {i}",
                        ["BillingCountry"] = "Hong Kong",
                        ["Contacts"] = new RecordsObject(Enumerable.Range(1, 2).Select(j =>
                            new AttributedObject("Contact", $"cont{Guid.NewGuid():N}")
                                {
                                    ["FirstName"] = $"UnitTest Contact B{(char)('a'+j)}",
                                    ["LastName"] = $"UnitTest Contact B{(char)('a'+j)}",
                                    ["MailingCountry"] = "Hong Kong"
                                }))
                    });

                var accountsResult = await client.Composite.CreateTreeAsync("Account", accounts.ToList())
                    .ConfigureAwait(false);
                accountsResult.Assert();
                Assert.NotEmpty(accountsResult.Results ?? new List<SaveResult>());
            }
            finally
            {
                await DeleteTestingRecords(client)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task CreateFailTest()
        {
            var client = await LoginTask()
                .ConfigureAwait(false);
            await DeleteTestingRecords(client)
                .ConfigureAwait(false);

            try
            {
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var result = await client.CreateAsync("Account", new JObject())
                        .ConfigureAwait(false);
                    result.Assert();
                });
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var obj = new AttributedObject($"UnknownObject{Guid.NewGuid():N}");
                    var request = new CreateRequest { Records = new List<IAttributedObject> { obj } };
                    var result = await client.CreateAsync($"UnknownObject{Guid.NewGuid():N}", request)
                        .ConfigureAwait(false);
                    result.Assert();
                });
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var prd2List = Enumerable.Range(1, 4000).Select(_ => new AttributedObject($"UnknownObject{Guid.NewGuid():N}"));
                    var result = await client.Composite.CreateAsync(prd2List.ToList())
                        .ConfigureAwait(false);
                    result.Assert();
                });
            }
            finally
            {
                await DeleteTestingRecords(client)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task RetrieveTest()
        {
            var client = await LoginTask()
                .ConfigureAwait(false);

            await DeleteTestingRecords(client)
                .ConfigureAwait(false);
            try
            {
                var caseList = Enumerable.Range(1, 5678).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}"
                }).ToArray();
                var result1 = await client.Composite.CreateAsync(caseList)
                    .ConfigureAwait(false);
                result1.Assert();

                //var externalIds = Enumerable.Range(1, 4000).Select(i => $"UnitTest{i}").ToArray();
                var result = await client.QueryAsync(@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%'").FirstAsync()
                    .ConfigureAwait(false);
                //var externalIds = Enumerable.Range(1, 4000).Select(i => new JObject { ["ProductCode"] = $"UnitTest{i}" }).ToArray();
                Assert.NotEmpty(result.Records ?? new List<JObject>());
                //Assert.NotEmpty(result.Collections());
                //var ids = result.Objects().Values.Select(c => c["Id"].ToString()).ToArray();
                var result2 = await client.Composite.RetrieveAsync("Case",
                    await client.QueryByLocatorAsync(result).Pull().Select(c => c["Id"].ToString()).ToListAsync()
                        .ConfigureAwait(false),
                    "Subject")
                    .ConfigureAwait(false);
                result2.Assert();
            }
            finally
            {
                await DeleteTestingRecords(client)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task UpdateTest()
        {
            var client = await LoginTask()
                .ConfigureAwait(false);

            await DeleteTestingRecords(client)
                .ConfigureAwait(false);
            try
            {
                var caseList = Enumerable.Range(1, 100).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}"
                }).ToArray();
                var result1 = await client.Composite.CreateAsync(caseList)
                    .ConfigureAwait(false);
                result1.Assert();

                await client.UpdateAsync("Case",
                    result1.SuccessResponses().Select(r => r.Value.Id).FirstOrDefault() ?? string.Empty,
                    new JObject { ["Description"] = "UnitTest0" })
                    .ConfigureAwait(false);

                var resultQuery = await client.QueryAsync(@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%' ORDER BY Id").Pull().ToListAsync()
                    .ConfigureAwait(false);

                var timer1 = Stopwatch.StartNew();
                _ = await client.Composite.UpdateAsync(resultQuery
                    .Select((c, i) => Dnf.Assign(c, new JObject { ["Description"] = $"UnitTest{i}" }))
                    .ToList())
                    .ConfigureAwait(false);
                timer1.Stop();

                WriteLine($"time1: {timer1.Elapsed.TotalSeconds}.");
            }
            finally
            {
                await DeleteTestingRecords(client)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task UpsertTest()
        {
            var client = await LoginTask()
                .ConfigureAwait(false);

            await DeleteTestingRecords(client)
                .ConfigureAwait(false);
            try
            {
                var createResult = await client
                    .CreateAsync("Product2", Dnf.Assign(GetTestProduct2(), new JObject
                    {
                        ["ProductCode"] = "UnitTest/0"
                    }))
                    .ConfigureAwait(false);
                createResult.Assert();

                var uniqueText = $"{Guid.NewGuid():N}";

                _ = await client.UpsertExternalAsync("Product2", "Id", createResult.Id ?? string.Empty,
                    new JObject { ["Name"] = $"UnitTest{uniqueText}" })
                    .ConfigureAwait(false);

                var updated = await client.RetrieveExternalAsync("Product2", "Id", createResult.Id ?? string.Empty)
                    .ConfigureAwait(false);
                Assert.Equal("UnitTest/0", updated?["ProductCode"]?.ToString());
            }
            finally
            {
                await DeleteTestingRecords(client)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            var client = await LoginTask()
                .ConfigureAwait(false);

            await DeleteTestingRecords(client)
                .ConfigureAwait(false);
            try
            {
                var caseList = Enumerable.Range(1, 100).Select(i => Dnf.Assign(GetTestProduct2(), new JObject
                {
                    ["ProductCode"] = $"UnitTest/{i}"
                })).ToArray();
                var resultCreate = await client.Composite.CreateAsync(caseList)
                    .ConfigureAwait(false);
                resultCreate.Assert();

                var del1Result = await client.DeleteAsync("Product2", resultCreate.SuccessResponses()
                        .Select(r => r.Value.Id).FirstOrDefault() ?? string.Empty)
                    .ConfigureAwait(false);
                Assert.True(del1Result);

                var delExtResult = await client.DeleteExternalAsync("Product2", "Id", 
                    Convert.ToString(resultCreate.Results().Values.Select(i => i["id"]).ElementAtOrDefault(1)) ?? string.Empty)
                    .ConfigureAwait(false);
                Assert.True(delExtResult);

                var resultQuery = await client.QueryAsync(@"
SELECT Id FROM Product2 WHERE ProductCode LIKE 'UnitTest%'").FirstAsync()
                    .ConfigureAwait(false);
                WriteLine($"total: {resultQuery.TotalSize}.");
                Assert.NotEmpty(resultQuery.Records ?? new List<JObject>());

                var timer1 = Stopwatch.StartNew();
                var result2 = await client.Composite.DeleteAsync(await client.QueryByLocatorAsync(resultQuery).Pull()
                    .Select(r => r["Id"]?.ToString()).ToListAsync()
                    .ConfigureAwait(false));
                timer1.Stop();
                result2.Assert();

                WriteLine($"time1: {timer1.Elapsed.TotalSeconds}.");
            }
            finally
            {
                await DeleteTestingRecords(client)
                    .ConfigureAwait(false);
            }
        }
    }
}
