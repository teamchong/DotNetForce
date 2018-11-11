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
    public class DNFClientCRUDTest : DNFClientTestBase
    {
        public DNFClientCRUDTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task CreateTest()
        {
            var client = await LoginTask;

            await DeleteTestingRecords(client);

            try
            {
                var result1 = await client.CreateAsync("Case", new JObject
                {
                    ["Subject"] = $"UnitTest0"
                });
                DNF.ThrowIfError(result1);

                var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}",
                }).ToArray();
                var result = await client.Composite.CreateAsync(caseList, true);
                DNF.ThrowIfError(result);
                Assert.NotEmpty(result.SuccessResponses());

                var expected = 200 * 26;
                var existingCase = await client.QueryAsync<JObject>($@"
SELECT Name FROM Opportunity ORDER BY Id LIMIT {expected}");

                var newCase = await client.Composite.CreateAsync(client.GetEnumerable(existingCase)
                    .Select(c => new AttributedObject("Case") { ["Subject"] = $"UnitTest{c["Name"]}" }));
                DNF.ThrowIfError(newCase);
                Assert.NotEmpty(newCase.SuccessResponses());

                var obj = new AttributedObject("Account", $"acc{Guid.NewGuid():N}")
                {
                    ["Name"] = $"UnitTest Account {Guid.NewGuid():N}",
                    ["BillingCountry"] = "Hong Kong",
                    ["Contacts"] = new RecordsObject(Enumerable.Range(1, 2).Select(j =>
                         new AttributedObject("Contact", $"cont{Guid.NewGuid():N}")
                         { ["LastName"] = $"UnitTest Contact{Guid.NewGuid():N}", ["MailingCountry"] = "Hong Kong" }
                         ))
                };
                var createTreeResult = await client
                    .CreateAsync($"Account", new RecordsObject(new[] { obj }).ToCreateRequest());
                DNF.ThrowIfError(createTreeResult);
                Assert.NotEmpty(createTreeResult.Results);

                var accounts =
                    Enumerable.Range(1, 6).Select(i => new AttributedObject("Account", $"acc{i}")
                    {
                        ["Name"] = $"UnitTest Account {i}",
                        ["BillingCountry"] = "Hong Kong",
                        ["Contacts"] = new RecordsObject(Enumerable.Range(1, 2).Select(j =>
                            new AttributedObject("Contact", $"cont{Guid.NewGuid():N}")
                            { ["LastName"] = $"UnitTest Contact{Guid.NewGuid():N}", ["MailingCountry"] = "Hong Kong" }))
                    });

                var accountsResult = await client.Composite.CreateTreeAsync("Account", accounts);
                DNF.ThrowIfError(accountsResult);
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
            var client = await LoginTask;
            await DeleteTestingRecords(client);

            try
            {
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var result = await client.CreateAsync($"Account", new JObject());
                    DNF.ThrowIfError(result);
                });
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var obj = new AttributedObject($"UnknownObject{Guid.NewGuid():N}");
                    var request = new CreateRequest { Records = new List<IAttributedObject> { obj } };
                    var result = await client.CreateAsync($"UnknownObject{Guid.NewGuid():N}", request);
                    DNF.ThrowIfError(result);
                });
                await Assert.ThrowsAsync<AggregateException>(async () =>
                {
                    var prd2List = Enumerable.Range(1, 4000).Select(i => new AttributedObject($"UnknownObject{Guid.NewGuid():N}"));
                    var result = await client.Composite.CreateAsync(prd2List);
                    DNF.ThrowIfError(result);
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
            var client = await LoginTask;

            await DeleteTestingRecords(client);
            try
            {
                var caseList = Enumerable.Range(1, 5678).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}",
                }).ToArray();
                var result1 = await client.Composite.CreateAsync(caseList);
                DNF.ThrowIfError(result1);

                //var externalIds = Enumerable.Range(1, 4000).Select(i => $"UnitTest{i}").ToArray();
                var result = await client.QueryAsync<JObject>($@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%'");
                //var externalIds = Enumerable.Range(1, 4000).Select(i => new JObject { ["Source_Product_ID__c"] = $"UnitTest{i}" }).ToArray();
                Assert.NotEmpty(result.Records);
                //Assert.NotEmpty(result.Collections());
                //var ids = result.Objects().Values.Select(c => c["Id"].ToString()).ToArray();
                var result2 = await client.Composite.RetrieveAsync("Case", client.GetEnumerable(result).Select(c => c["Id"].ToString()), "Subject");
                DNF.ThrowIfError(result2);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task UpdateTest()
        {
            var client = await LoginTask;

            await DeleteTestingRecords(client);
            try
            {
                var caseList = Enumerable.Range(1, 100000).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}",
                }).ToArray();
                var result1 = await client.Composite.CreateAsync(caseList);
                DNF.ThrowIfError(result1);

                await client.UpdateAsync("Case",
                    result1.SuccessResponses().Select(r => r.Value.Id).FirstOrDefault(),
                    new JObject { ["Description"] = "UnitTest0" });

                var resultQuery = await client.QueryAsync<JObject>($@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%' ORDER BY Id");

                var timer1 = System.Diagnostics.Stopwatch.StartNew();
                var result2 = await client.Composite.UpdateAsync(client.GetEnumerable(resultQuery)
                    .Select((c, i) => DNF.Assign(c, new JObject { ["Description"] = $"UnitTest{i}" })));
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
            var client = await LoginTask;

            await DeleteTestingRecords(client);
            try
            {
                var createResult = await client.CreateAsync("Product2", DNF.Assign(GetTestProduct2(), new JObject
                {
                    ["Source_Product_ID__c"] = $"UnitTest/0"
                }));
                DNF.ThrowIfError(createResult);

                var uniqueText = $"{Guid.NewGuid():N}";

                var upserted = await client.UpsertExternalAsync("Product2", "Source_Product_ID__c", "UnitTest/0",
                    new JObject { ["Name"] = $"UnitTest{uniqueText}" });

                var updated = await client.RetrieveExternalAsync<JObject>("Product2", "Source_Product_ID__c", "UnitTest/0");
                Assert.Equal($"UnitTest/0", updated?["Source_Product_ID__c"]?.ToString());
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            var client = await LoginTask;

            await DeleteTestingRecords(client);
            try
            {
                var caseList = Enumerable.Range(1, 100000).Select(i => DNF.Assign(GetTestProduct2(), new JObject
                {
                    ["Source_Product_ID__c"] = $"UnitTest/{i}",
                })).ToArray();
                var resultCreate = await client.Composite.CreateAsync(caseList);
                DNF.ThrowIfError(resultCreate);

                var del1Result = await client.DeleteAsync("Product2", resultCreate.SuccessResponses().Select(r => r.Value.Id).FirstOrDefault());
                Assert.True(del1Result);

                var delExtResult = await client.DeleteExternalAsync("Product2", "Source_Product_ID__c", "UnitTest/2");
                Assert.True(delExtResult);

                var resultQuery = await client.QueryAsync<JObject>($@"
SELECT Id FROM Product2 WHERE Source_Product_ID__c LIKE 'UnitTest%'");
                WriteLine($"total: {resultQuery.TotalSize}.");
                Assert.NotEmpty(resultQuery.Records);

                var timer1 = System.Diagnostics.Stopwatch.StartNew();
                var result2 = await client.Composite.DeleteAsync(client.GetEnumerable(resultQuery).Select(r => r["Id"]?.ToString()));
                timer1.Stop();
                DNF.ThrowIfError(result2);

                WriteLine($"time1: {timer1.Elapsed.TotalSeconds}.");
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }
    }
}
