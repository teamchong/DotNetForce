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
            var client = await LoginTask();

            await DeleteTestingRecords(client);

            try
            {
                var result1 = await client.CreateAsync("Case", new JObject
                {
                    ["Subject"] = "UnitTest0"
                });
                Dnf.ThrowIfError(result1);

                var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}"
                }).ToArray();
                var result = await client.Composite.CreateAsync(caseList, true);
                Dnf.ThrowIfError(result);
                Assert.NotEmpty(result.SuccessResponses());

                var expected = 200 * 26;
                var existingCase = await client.QueryAsync($@"
SELECT Name FROM Opportunity ORDER BY Id LIMIT {expected}");

                var newCase = await client.Composite.CreateAsync(await client.GetAsyncEnumerable(existingCase)
                    .Select(c => new AttributedObject("Case") { ["Subject"] = $"UnitTest{c["Name"]}" }).ToListAsync().ConfigureAwait(false));
                Dnf.ThrowIfError(newCase);
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
                    .CreateAsync("Account", new RecordsObject(new[] { obj }).ToCreateRequest());
                Dnf.ThrowIfError(createTreeResult);
                Assert.NotEmpty(createTreeResult.Results);

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

                var accountsResult = await client.Composite.CreateTreeAsync("Account", accounts.ToList());
                Dnf.ThrowIfError(accountsResult);
                Assert.NotEmpty(accountsResult.Results);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task CreateFailTest()
        {
            var client = await LoginTask();
            await DeleteTestingRecords(client);

            try
            {
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var result = await client.CreateAsync("Account", new JObject());
                    Dnf.ThrowIfError(result);
                });
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var obj = new AttributedObject($"UnknownObject{Guid.NewGuid():N}");
                    var request = new CreateRequest { Records = new List<IAttributedObject> { obj } };
                    var result = await client.CreateAsync($"UnknownObject{Guid.NewGuid():N}", request);
                    Dnf.ThrowIfError(result);
                });
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var prd2List = Enumerable.Range(1, 4000).Select(_ => new AttributedObject($"UnknownObject{Guid.NewGuid():N}"));
                    var result = await client.Composite.CreateAsync(prd2List.ToList());
                    Dnf.ThrowIfError(result);
                });
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task RetrieveTest()
        {
            var client = await LoginTask();

            await DeleteTestingRecords(client);
            try
            {
                var caseList = Enumerable.Range(1, 5678).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}"
                }).ToArray();
                var result1 = await client.Composite.CreateAsync(caseList);
                Dnf.ThrowIfError(result1);

                //var externalIds = Enumerable.Range(1, 4000).Select(i => $"UnitTest{i}").ToArray();
                var result = await client.QueryAsync(@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%'");
                //var externalIds = Enumerable.Range(1, 4000).Select(i => new JObject { ["ProductCode"] = $"UnitTest{i}" }).ToArray();
                Assert.NotEmpty(result.Records);
                //Assert.NotEmpty(result.Collections());
                //var ids = result.Objects().Values.Select(c => c["Id"].ToString()).ToArray();
                var result2 = await client.Composite.RetrieveAsync("Case",
                    await client.GetAsyncEnumerable(result).Select(c => c["Id"].ToString()).ToListAsync().ConfigureAwait(false),
                    "Subject");
                Dnf.ThrowIfError(result2);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task UpdateTest()
        {
            var client = await LoginTask();

            await DeleteTestingRecords(client);
            try
            {
                var caseList = Enumerable.Range(1, 100).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}"
                }).ToArray();
                var result1 = await client.Composite.CreateAsync(caseList);
                Dnf.ThrowIfError(result1);

                await client.UpdateAsync("Case",
                    result1.SuccessResponses().Select(r => r.Value.Id).FirstOrDefault(),
                    new JObject { ["Description"] = "UnitTest0" });

                var resultQuery = await client.QueryAsync(@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%' ORDER BY Id");

                var timer1 = Stopwatch.StartNew();
                _ = await client.Composite.UpdateAsync(await client.GetAsyncEnumerable(resultQuery)
                    .Select((c, i) => Dnf.Assign(c, new JObject { ["Description"] = $"UnitTest{i}" }))
                    .ToListAsync().ConfigureAwait(false));
                timer1.Stop();

                WriteLine($"time1: {timer1.Elapsed.TotalSeconds}.");
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task UpsertTest()
        {
            var client = await LoginTask();

            await DeleteTestingRecords(client);
            try
            {
                var createResult = await client.CreateAsync("Product2", Dnf.Assign(GetTestProduct2(), new JObject
                {
                    ["ProductCode"] = "UnitTest/0"
                }));
                Dnf.ThrowIfError(createResult);

                var uniqueText = $"{Guid.NewGuid():N}";

                _ = await client.UpsertExternalAsync("Product2", "Id", createResult.Id,
                    new JObject { ["Name"] = $"UnitTest{uniqueText}" });

                var updated = await client.RetrieveExternalAsync("Product2", "Id", createResult.Id);
                Assert.Equal("UnitTest/0", updated?["ProductCode"]?.ToString());
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            var client = await LoginTask();

            await DeleteTestingRecords(client);
            try
            {
                var caseList = Enumerable.Range(1, 100).Select(i => Dnf.Assign(GetTestProduct2(), new JObject
                {
                    ["ProductCode"] = $"UnitTest/{i}"
                })).ToArray();
                var resultCreate = await client.Composite.CreateAsync(caseList);
                Dnf.ThrowIfError(resultCreate);

                var del1Result = await client.DeleteAsync("Product2", resultCreate.SuccessResponses().Select(r => r.Value.Id).FirstOrDefault());
                Assert.True(del1Result);

                var delExtResult = await client.DeleteExternalAsync("Product2", "Id", 
                    Convert.ToString(resultCreate.Results().Values.Select(i => i["id"]).ElementAtOrDefault(1)));
                Assert.True(delExtResult);

                var resultQuery = await client.QueryAsync(@"
SELECT Id FROM Product2 WHERE ProductCode LIKE 'UnitTest%'");
                WriteLine($"total: {resultQuery.TotalSize}.");
                Assert.NotEmpty(resultQuery.Records);

                var timer1 = Stopwatch.StartNew();
                var result2 = await client.Composite.DeleteAsync(await client.GetAsyncEnumerable(resultQuery)
                    .Select(r => r["Id"]?.ToString()).ToListAsync().ConfigureAwait(false));
                timer1.Stop();
                Dnf.ThrowIfError(result2);

                WriteLine($"time1: {timer1.Elapsed.TotalSeconds}.");
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }
    }
}
