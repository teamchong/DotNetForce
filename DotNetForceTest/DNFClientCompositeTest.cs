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
    public class DNFClientCompositeTest : DNFClientTestBase
    {
        public DNFClientCompositeTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task CompositeTest()
        {
            var client = await LoginTask;

            var uniqueId = $"{Guid.NewGuid():N}";
            try
            {
                // characters to avoid
                // " json injection
                // \ json & soql injection
                // ' soql injection, becaused string is quoted by '
                // / url injection, cannot upsert w/ external id
                // & url param injection, & need to be escaped
                // % url param param injection, % need to be escaped
                // + url param injection, converted to space in query
                // $ url param injection
                var testSubject = "UnitTest @!~-.,=_[]()*`";

                var request = new CompositeRequest(allOrNone: true);
                request.Create("1create", "Product2", GetTestProduct2().Assign(new JObject
                {
                    ["Name"] = $"UnitTest{Guid.NewGuid():N}",
                    ["Source_Product_ID__c"] = $"{testSubject}",
                }));
                request.Query("1query", $@"
SELECT Id, Source_Product_ID__c
FROM Product2
WHERE Id = '@{{1create.id}}'");/*
AND Source_Product_ID__c = {DNF.SOQLString($"{testSubject}")}");//*/
                request.Update("2update", "Product2", new JObject
                {
                    ["Id"] = $"@{{1query.records[0].Id}}",
                    //["Source_Product_ID__c"] = $"UnitTest{testSubject}2",
                    ["Source_Product_ID__c"] = $"@{{1query.records[0].Source_Product_ID__c}}2",
                });
                request.Retrieve("2retrieve", "Product2", $"@{{1query.records[0].Id}}", "Id", "Source_Product_ID__c");
                request.UpsertExternal("3upsert", "Product2", "Source_Product_ID__c", new JObject
                {
                    //["Id"] = $"@{{1query.records[0].Id}}",
                    ["Source_Product_ID__c"] = $"@{{2retrieve.Source_Product_ID__c}}",
                    ["Venue_ID__c"] = "U2"
                });
                request.Query("3_query", $@"
SELECT Id, Source_Product_ID__c
FROM Product2
WHERE Id = '@{{2retrieve.Id}}'
AND Source_Product_ID__c = '@{{2retrieve.Source_Product_ID__c}}'
AND Venue_ID__c = 'U2'");
                request.Delete("4delete", "Product2", $"@{{3_query.records[0].Id}}");
                var result = await client.Composite.PostAsync(request);
                Output.WriteLine(result.ToString());
                result.ThrowIfError();
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task CompositeUrlInjectionTest()
        {
            var client = await LoginTask;

            await DeleteTestingRecords(client);
            try
            {
                await Assert.ThrowsAsync<AggregateException>(async () =>
                {
                    var request = new CompositeRequest(allOrNone: true);
                    request.Create("create", "Product2", GetTestProduct2().Assign(new JObject
                    {
                        ["Name"] = $"UnitTest/",
                        ["Source_Product_ID__c"] = $"UnitTest/",
                    }));
                    request.Query("query", @"
SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{create.id}'");
                    request.Query("query2", @"
SELECT Id, Name, Source_Product_ID__c FROM Product2
WHERE Id = '@{query.records[0].Id}'
AND Source_Product_ID__c = '@{query.records[0].Source_Product_ID__c}'");
                    request.Update("update", "Product2", new JObject
                    {
                        ["Id"] = "@{query2.records[0].Id}",
                        ["Source_Product_ID__c"] = "@{query2.records[0].Source_Product_ID__c}2",
                    });
                    request.Query("updated", @"
SELECT Id, Name, Source_Product_ID__c FROM Product2
WHERE Id = '@{query2.records[0].Id}'
AND Source_Product_ID__c = '@{query2.records[0].Source_Product_ID__c}2");
                    request.UpsertExternal("upsert", "Product2", "Source_Product_ID__c", "@{query.records[0].Source_Product_ID__c}", new JObject
                    {
                        ["Name"] = "@{query.records[0].Name}",
                    });
                    request.Delete("delete", "Product2", "@{updated.records[0].Id}");
                    var result = await client.Composite.PostAsync(request);
                    result.ThrowIfError();
                });
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task CompositeSoqlInjectionTest()
        {
            var client = await LoginTask;

            await DeleteTestingRecords(client);
            try
            {
                var testText = $"UnitTest' AND Name != '";
                var request = new CompositeRequest();
                request.Create("create", "Product2", GetTestProduct2().Assign(new JObject
                {
                    ["Source_Product_ID__c"] = testText,
                }));
                request.Retrieve("created", "Product2", "@{create.id}");

                request.Query("query", @"
SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{created.Id}'
AND Source_Product_ID__c = '@{created.Source_Product_ID__c}'");

                request.UpsertExternal("upsert", "Product2", "Source_Product_ID__c", "@{created.Source_Product_ID__c}", new JObject
                {
                    ["Name"] = testText,
                });
                request.Query("upserted", $@"
SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{{created.Id}}'
AND Name = {DNF.SOQLString(testText)}");

                request.Update("update", "Product2", new JObject
                {
                    ["Id"] = "@{created.Id}",
                    ["Source_Product_ID__c"] = "@{created.Source_Product_ID__c}",
                });
                request.Query("updated", $@"
SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{{created.Id}}'
AND Source_Product_ID__c = {DNF.SOQLString(testText)}");

                request.Delete("delete", "Product2", "@{create.id}");
                var result = await client.Composite.PostAsync(request);
                result.ThrowIfError();
                Assert.Equal(testText, result.Results("created")?["Source_Product_ID__c"]);
                Assert.Equal(0, result.Queries("query")?.TotalSize);
                Assert.Equal(testText, result.Queries("upserted")?.Records?[0]?["Name"]);
                Assert.Equal(testText.Replace("\n", " ").Replace("\\", ""), result.Queries("updated")?.Records?[0]?["Name"]);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
            try
            {
                var testText = $"Uni\\nt\\tTe\\\\ns\\\\ts\\\\\\nt\\\\\\tt";
                var request = new CompositeRequest();
                request.Create("create", "Product2", GetTestProduct2().Assign(new JObject
                {
                    ["Source_Product_ID__c"] = testText,
                }));
                request.Retrieve("created", "Product2", "@{create.id}");

                request.Query("query", @"
SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{created.Id}'
AND Source_Product_ID__c = '@{created.Source_Product_ID__c}'");

                // cannot upsert
                request.UpsertExternal("upsert", "Product2", "Source_Product_ID__c", "@{created.Source_Product_ID__c}", new JObject
                {
                    ["Name"] = testText,
                });
                request.Query("upserted", $@"
SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{{created.Id}}'
AND Name = {DNF.SOQLString(testText)}");

                request.Update("update", "Product2", new JObject
                {
                    ["Id"] = "@{created.Id}",
                    ["Source_Product_ID__c"] = "@{created.Source_Product_ID__c}",
                });
                request.Retrieve("updated", "Product2", $"@{{created.Id}}");

                request.Delete("delete", "Product2", "@{create.id}");
                var result = await client.Composite.PostAsync(request);
                //result.ThrowIfError();
                Assert.Equal(testText, result.Results("created")?["Source_Product_ID__c"]);
                Assert.Equal(0, result.Queries("query")?.TotalSize);
                Assert.Equal(0, result.Queries("upserted").TotalSize);
                Assert.Equal("UninttTe s\ts t\tt", result.Results("updated")?["Source_Product_ID__c"]);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task CompositeUrlParamInjectionTest()
        {
            var client = await LoginTask;

            await DeleteTestingRecords(client);
            try
            {
                var request = new CompositeRequest(allOrNone: true);
                request.Create("create", "Product2", GetTestProduct2().Assign(new JObject
                {
                    ["Name"] = $"UnitTest' AND Name != '",
                    ["Source_Product_ID__c"] = $"UnitTest' AND Name != '",
                }));
                request.Query("query", @"
SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{create.id}'");
                request.Query("query2", @"
SELECT Id, Name, Source_Product_ID__c FROM Product2
WHERE Id = '@{query.records[0].Id}'
AND Source_Product_ID__c = '@{query.records[0].Source_Product_ID__c}'");
                request.Update("update", "Product2", new JObject
                {
                    ["Id"] = "@{query.records[0].Id}",
                    ["Source_Product_ID__c"] = "@{query.records[0].Source_Product_ID__c}2",
                });
                request.Query("updated", $@"
SELECT Id, Name, Source_Product_ID__c FROM Product2
WHERE Id = '@{{query.records[0].Id}}'
AND Source_Product_ID__c = {DNF.SOQLString("UnitTest' AND Name != '2")}");
                request.UpsertExternal("upsert", "Product2", "Source_Product_ID__c", "UnitTest' AND Name != '2", new JObject
                {
                    ["Name"] = "@{query.records[0].Name}",
                });
                request.Delete("delete", "Product2", "@{updated.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                result.ThrowIfError();
                Assert.Equal(0, result.Queries("query2")?.TotalSize);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task CompositeJsonInjectionTest()
        {
            var client = await LoginTask;

            try
            {
                var request = new CompositeRequest(allOrNone: true);
                request.Create("create", "Case", new JObject
                {
                    ["Subject"] = "UnitTest\",\"SuppliedName\":\"D"
                });
                request.Query("query", @"
SELECT Id, Subject, SuppliedName FROM Case WHERE Id = '@{create.id}'");
                request.Query("query2", @"
SELECT Id, Subject, SuppliedName FROM Case
WHERE Id = '@{query.records[0].Id}'
AND Subject = '@{query.records[0].Subject}'");
                request.Update("update", "Case", new JObject
                {
                    ["Id"] = "@{query2.records[0].Id}",
                    ["Subject"] = "@{query2.records[0].Subject}",
                });
                request.Query("updated", @"
SELECT Id, Subject, SuppliedName FROM Case
WHERE Id = '@{create.id}'
AND Subject = 'UnitTest'
AND SuppliedName = 'D'");
                request.Delete("delete", "Case", "@{updated.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                result.ThrowIfError();
            }
            finally
            {
                await DeleteTestingRecords(client);
            }

            try
            {
                var request = new CompositeRequest(allOrNone: true);
                request.Create("create", "Case", new JObject
                {
                    ["Subject"] = "UnitTest\\"
                });
                request.Query("query", @"
SELECT Id, Subject, SuppliedName FROM Case WHERE Id = '@{create.id}'");
                request.Query("query2", @"
SELECT Id, Subject, SuppliedName FROM Case
WHERE Id = '@{query.records[0].Id}'
AND Subject = '@{query.records[0].Subject}'");
                request.Update("update", "Case", new JObject
                {
                    ["Id"] = "@{query2.records[0].Id}",
                    ["Subject"] = "@{query2.records[0].Subject}\"",
                });
                request.Delete("delete", "Case", "@{updated.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                result.ThrowIfError();
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task CompositeFailTest()
        {
            var client = await LoginTask;

            // characters to avoid
            // & break query string i.e. q=... MALFORMED_QUERY
            // + break query string, my guess: converted to space
            // ' upsert failed, not found
            // / upsert failed, not found
            // % transaction all rollback
            // \ transaction all rollback
            // $ transaction all rollback


            await Assert.ThrowsAsync<AggregateException>(async () =>
            {
                var uniqueId = $"{Guid.NewGuid():N}";

                var testName = $"UnitTest$@\\/ %!~-[].,()`=&+*_\"'{{}}";

                var request = new CompositeRequest(allOrNone: true);
                request.Create("create", "Case", new JObject
                {
                    ["Subject"] = $"{testName}",
                });
                request.Query("query1", $@"
SELECT Id, Subject
FROM Case
WHERE Id = '@{{create.id}}'
AND Subject = {DNF.SOQLString($"{testName}")}");
                request.Update("update", "Case", new JObject
                {
                    ["Id"] = $"@{{create.id}}",
                    ["Subject"] = $"UnitTest{testName}",
                });
                request.Query("query2", $@"
SELECT Id, Subject
FROM Case
WHERE Id = '@{{create.id}}'
AND Subject = {DNF.SOQLString($"UnitTest{testName}")}");
                request.UpsertExternal("upsert", "Case", "Id", new JObject
                {
                    ["Id"] = $"@{{query.records[0].Id}}",
                    ["Subject"] = $"UnitTest {testName}",
                });
                request.Query("query3", $@"
SELECT Id, Subject
FROM Case
WHERE Id = '@{{create.id}}'
AND Subject = {DNF.SOQLString($"UnitTest {testName}")}");
                request.Delete("delete", "Case", $"1@{{query2.records[0].Id}}");
                var result = await client.Composite.PostAsync(request);
                Output.WriteLine(result.ToString());
                Assert.NotNull(result.Errors("query3"));
                Assert.Equal(
                    result.Requests().Count - 1,
                    result.Errors().Values
                    .Count(errs => errs.Any(err => err.Message == "The transaction was rolled back since another operation in the same transaction failed.")));
                result.ThrowIfError();
            });
        }
    }
}
