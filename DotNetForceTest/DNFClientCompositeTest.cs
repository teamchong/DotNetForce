using DotNetForce;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DotNetForceTest
{
    public class DnfClientCompositeTest : DnfClientTestBase
    {
        public DnfClientCompositeTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task CompositeTest()
        {
            var client = await LoginTask();
            
            try
            {
                // characters to avoid
                // " json injection
                // \ json & soql injection
                // ' soql injection, because string is quoted by '
                // / url injection, cannot upsert w/ external id
                // & url param injection, & need to be escaped
                // % url param param injection, % need to be escaped
                // + url param injection, converted to space in query
                // $ url param injection
                var testSubject = "UnitTest";// @!~-.,=_[]()*`";

                var request = new CompositeRequest(true);
                request.Create("1create", "Product2", Dnf.Assign(GetTestProduct2(), new JObject
                {
                    ["Name"] = $"UnitTest{Guid.NewGuid():N}",
                    ["ProductCode"] = $"{testSubject}",
                    ["CurrencyIsoCode"] = "USD"
                }));
                request.Query("1query", @"
SELECT Id, ProductCode
FROM Product2
WHERE Id = '@{1create.id}'"); /*
AND ProductCode = {Dnf.SOQLString($"{testSubject}")}");//*/
                request.Update("2update", "Product2", new JObject
                {
                    ["Id"] = "@{1query.records[0].Id}",
                    //["ProductCode"] = $"UnitTest{testSubject}2",
                    ["ProductCode"] = "@{1query.records[0].ProductCode}2"
                });
                request.Retrieve("2retrieve", "Product2", "@{1query.records[0].Id}", "Id", "ProductCode");
                request.UpsertExternal("3upsert", "Product2", "Id", new JObject
                {
                    //["Id"] = $"@{{1query.records[0].Id}}",
                    ["Id"] = "@{2retrieve.Id}",
                    ["ExternalId"] = "U2"
                });
                request.Query("3_query", @"
SELECT Id, ProductCode
FROM Product2
WHERE Id = '@{2retrieve.Id}'
AND ProductCode = '@{2retrieve.ProductCode}'
AND ExternalId = 'U2'");
                request.Delete("4delete", "Product2", "@{3_query.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                WriteLine(result.ToString());
                Dnf.ThrowIfError(result);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task CompositeUrlInjectionTest()
        {
            var client = await LoginTask();

            await DeleteTestingRecords(client);
            try
            {
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var request = new CompositeRequest(true);
                    request.Create("create", "Product2", Dnf.Assign(GetTestProduct2(), new JObject
                    {
                        ["Name"] = "UnitTest/",
                        ["ProductCode"] = "UnitTest/",
                        ["CurrencyIsoCode"] = "USD"
                    }));
                    request.Query("query", @"
SELECT Id, Name, ProductCode FROM Product2 WHERE Id = '@{create.id}'");
                    request.Query("query2", @"
SELECT Id, Name, ProductCode FROM Product2
WHERE Id = '@{query.records[0].Id}'
AND ProductCode = '@{query.records[0].ProductCode}'");
                    request.Update("update", "Product2", new JObject
                    {
                        ["Id"] = "@{query2.records[0].Id}",
                        ["ProductCode"] = "@{query2.records[0].ProductCode}2"
                    });
                    request.Query("updated", @"
SELECT Id, Name, ProductCode FROM Product2
WHERE Id = '@{query2.records[0].Id}'
AND ProductCode = '@{query2.records[0].ProductCode}2");
                    request.UpsertExternal("upsert", "Product2", "Id", "@{query.records[0].Id}", new JObject
                    {
                        ["Name"] = "@{query.records[0].Name}"
                    });
                    request.Delete("delete", "Product2", "@{updated.records[0].Id}");
                    var result = await client.Composite.PostAsync(request);
                    Dnf.ThrowIfError(result);
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
            var client = await LoginTask();

            await DeleteTestingRecords(client);
            try
            {
                var testText = "UnitTest' AND Name != '";
                var request = new CompositeRequest();
                request.Create("create", "Product2", Dnf.Assign(GetTestProduct2(), new JObject
                {
                    ["ProductCode"] = testText
                }));
                request.Retrieve("created", "Product2", "@{create.id}");

                request.Query("query", @"
SELECT Id, Name, ProductCode FROM Product2 WHERE Id = '@{created.Id}'
AND ProductCode = '@{created.ProductCode}'");

                request.UpsertExternal("upsert", "Product2", "Id", "@{created.Id}", new JObject
                {
                    ["Name"] = testText
                });
                request.Query("upserted", $@"
SELECT Id, Name, ProductCode FROM Product2 WHERE Id = '@{{created.Id}}'
AND Name = {Dnf.SoqlString(testText)}");

                request.Update("update", "Product2", new JObject
                {
                    ["Id"] = "@{created.Id}",
                    ["ProductCode"] = "@{created.ProductCode}"
                });
                request.Query("updated", $@"
SELECT Id, Name, ProductCode FROM Product2 WHERE Id = '@{{created.Id}}'
AND ProductCode = {Dnf.SoqlString(testText)}");

                request.Delete("delete", "Product2", "@{create.id}");
                var result = await client.Composite.PostAsync(request);
                Dnf.ThrowIfError(result);
                Assert.Equal(testText, result.Results("created")?["ProductCode"]);
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
                var testText = "Uni\\nt\\tTe\\\\ns\\\\ts\\\\\\nt\\\\\\tt";
                var request = new CompositeRequest();
                request.Create("create", "Product2", Dnf.Assign(GetTestProduct2(), new JObject
                {
                    ["ProductCode"] = testText
                }));
                request.Retrieve("created", "Product2", "@{create.id}");

                request.Query("query", @"
SELECT Id, Name, ProductCode FROM Product2 WHERE Id = '@{created.Id}'
AND ProductCode = '@{created.ProductCode}'");

                // cannot upsert
                request.UpsertExternal("upsert", "Product2", "Id", "@{created.Id}", new JObject
                {
                    ["Name"] = testText
                });
                request.Query("upserted", $@"
SELECT Id, Name, ProductCode FROM Product2 WHERE Id = '@{{created.Id}}'
AND Name = {Dnf.SoqlString(testText)}");

                request.Update("update", "Product2", new JObject
                {
                    ["Id"] = "@{created.Id}",
                    ["ProductCode"] = "@{created.ProductCode}"
                });
                request.Retrieve("updated", "Product2", "@{created.Id}");

                request.Delete("delete", "Product2", "@{create.id}");
                var result = await client.Composite.PostAsync(request);
                //Dnf.ThrowIfError(result);
                Assert.Equal(testText, result.Results("created")?["ProductCode"]);
                Assert.Equal(0, result.Queries("query")?.TotalSize);
                Assert.Equal(1, result.Queries("upserted").TotalSize);
                Assert.Equal("Uni t\tTe\\ns\\ts\\ t\\\tt", (string)result.Results("updated")?["ProductCode"]);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }
        }

        [Fact]
        public async Task CompositeUrlParamInjectionTest()
        {
            var client = await LoginTask();

            await DeleteTestingRecords(client);
            try
            {
                var request = new CompositeRequest(true);
                request.Create("create", "Product2", Dnf.Assign(GetTestProduct2(), new JObject
                {
                    ["Name"] = "UnitTest' AND Name != '",
                    ["ProductCode"] = "UnitTest' AND Name != '",
                    ["CurrencyIsoCode"] = "USD"
                }));
                request.Query("query", @"
SELECT Id, Name, ProductCode FROM Product2 WHERE Id = '@{create.id}'");
                request.Query("query2", @"
SELECT Id, Name, ProductCode FROM Product2
WHERE Id = '@{query.records[0].Id}'
AND ProductCode = '@{query.records[0].ProductCode}'");
                request.Update("update", "Product2", new JObject
                {
                    ["Id"] = "@{query.records[0].Id}",
                    ["ProductCode"] = "@{query.records[0].ProductCode}2"
                });
                request.Query("updated", $@"
SELECT Id, Name, ProductCode FROM Product2
WHERE Id = '@{{query.records[0].Id}}'
AND ProductCode = {Dnf.SoqlString("UnitTest' AND Name != '2")}");
                //request.UpsertExternal("upsert", "Product2", "Id", "@{updated.records[0].Id}' AND Name != '2", new JObject
                //{
                //    ["Name"] = "@{query.records[0].Name}"
                //});
                request.Delete("delete", "Product2", "@{updated.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                Dnf.ThrowIfError(result);
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
            var client = await LoginTask();

            try
            {
                var request = new CompositeRequest(true);
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
                    ["Subject"] = "@{query2.records[0].Subject}"
                });
                request.Query("updated", @"
SELECT Id, Subject, SuppliedName FROM Case
WHERE Id = '@{create.id}'
AND Subject = 'UnitTest'
AND SuppliedName = 'D'");
                request.Delete("delete", "Case", "@{updated.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                Dnf.ThrowIfError(result);
            }
            finally
            {
                await DeleteTestingRecords(client);
            }

            await Assert.ThrowsAsync<ForceException>(async () =>
            {
                var request = new CompositeRequest(true);
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
                    ["Subject"] = "@{query2.records[0].Subject}\""
                });
                request.Delete("delete", "Case", "@{updated.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                Dnf.ThrowIfError(result);
            });
        }

        [Fact]
        public async Task CompositeFailTest()
        {
            var client = await LoginTask();

            // characters to avoid
            // & break query string i.e. q=... MALFORMED_QUERY
            // + break query string, my guess: converted to space
            // ' upsert failed, not found
            // / upsert failed, not found
            // % transaction all rollback
            // \ transaction all rollback
            // $ transaction all rollback


            await Assert.ThrowsAsync<ForceException>(async () =>
            {
                var testName = "UnitTest$@\\/ %!~-[].,()`=&+*_\"'{}";

                var request = new CompositeRequest(true);
                request.Create("create", "Case", new JObject
                {
                    ["Subject"] = $"{testName}"
                });
                request.Query("query1", $@"
SELECT Id, Subject
FROM Case
WHERE Id = '@{{create.id}}'
AND Subject = {Dnf.SoqlString($"{testName}")}");
                request.Update("update", "Case", new JObject
                {
                    ["Id"] = "@{create.id}",
                    ["Subject"] = $"UnitTest{testName}"
                });
                request.Query("query2", $@"
SELECT Id, Subject
FROM Case
WHERE Id = '@{{create.id}}'
AND Subject = {Dnf.SoqlString($"UnitTest{testName}")}");
                request.UpsertExternal("upsert", "Case", "Id", new JObject
                {
                    ["Id"] = "@{query.records[0].Id}",
                    ["Subject"] = $"UnitTest {testName}"
                });
                request.Query("query3", $@"
SELECT Id, Subject
FROM Case
WHERE Id = '@{{create.id}}'
AND Subject = {Dnf.SoqlString($"UnitTest {testName}")}");
                request.Delete("delete", "Case", "1@{query2.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                WriteLine(result.ToString());
                Assert.NotNull(result.Errors("query3"));
                Assert.Equal(
                    result.Requests().Count - 1,
                    result.Errors().Values
                        .Count(errs => errs.Any(err => err.Message == "The transaction was rolled back since another operation in the same transaction failed.")));
                Dnf.ThrowIfError(result);
            });
        }
    }
}
