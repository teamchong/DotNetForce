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
    public class DNFClientTest
    {
        private readonly ITestOutputHelper Output;
        private Uri LoginUri { get; set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string UserName { get; set; }
        private string Password { get; set; }

        public DNFClientTest(ITestOutputHelper output)
        {
            Output = output;

            var dirPath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            while (dirPath.Name != "bin" && dirPath.Parent != null) dirPath = dirPath.Parent;
            dirPath = dirPath.Parent;
            var targetPath = dirPath?.Parent?.Parent?.FullName;
            var jsonFile = targetPath == null ? null : Path.Combine(targetPath, "LoginProfiles.json");
            if (jsonFile != null && !File.Exists(jsonFile))
            {
                var projectPath = dirPath.FullName;
                jsonFile = Path.Combine(projectPath, "LoginProfiles.json");
            }
            var loginProfile = JObject.Parse(File.ReadAllText(jsonFile));
            LoginUri = new Uri(loginProfile["DEV"]["LoginUrl"].ToObject<string>());
            ClientId = loginProfile["DEV"]["ClientId"].ToObject<string>();
            ClientSecret = loginProfile["DEV"]["ClientSecret"].ToObject<string>();
            UserName = loginProfile["DEV"]["UserName"].ToObject<string>();
            Password = loginProfile["DEV"]["Password"].ToObject<string>();
        }

        private async Task DeleteTestingRecords(DNFClient client)
        {
            (await (await client.QueryAsync<JObject>($@"
SELECT Id FROM Case WHERE Subject LIKE 'UnitTest%'")).ToEnumerable(client)
.Select(r => r["Id"]?.ToObject<string>()).DeleteAsync(client)).ThrowIfError();
            (await (await client.QueryAsync<JObject>($@"
SELECT Id FROM Account WHERE Name LIKE 'UnitTest%'")).ToEnumerable(client)
.Select(r => r["Id"]?.ToObject<string>()).DeleteAsync(client)).ThrowIfError();
            (await (await client.QueryAsync<JObject>($@"
SELECT Id FROM Contact WHERE Name LIKE 'UnitTest%'")).ToEnumerable(client)
.Select(r => r["Id"]?.ToObject<string>()).DeleteAsync(client)).ThrowIfError();
            (await (await client.QueryAsync<JObject>($@"
SELECT Id FROM Product2 WHERE Source_Product_ID__c LIKE 'UnitTest%'")).ToEnumerable(client)
.Select(r => r["Id"]?.ToObject<string>()).DeleteAsync(client)).ThrowIfError();
        }

        private JObject GetTestProduct2()
        {
            var id = Guid.NewGuid();
            return new JObject
            {
                ["Name"] = $"UnitTest{id:N}",
                ["Contract_Type_ID__c"] = "U",
                ["Contract_Type__c"] = "U",
                ["EMS_Rate_ID__c"] = "U",
                ["EMS_Rate_Unique_ID__c"] = 0,
                ["EMS_Sub_Type_ID__c"] = "U",
                ["Venue_ID__c"] = "U",
                ["Zone_ID__c"] = "U",
                ["Source_Product_ID__c"] = $"UnitTest{id:N}",
            };
        }

        private void WriteLine(string message)
        {
            var msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{message}";
            System.Diagnostics.Debug.WriteLine(msg);
            Output.WriteLine(msg);
        }

        [Fact]
        public async Task LoginTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
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
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
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
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
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
                var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
                var oppty = await client.QueryAsync<JObject>($@"
SELECT Id, UnkownField FROM Opportunity LIMIT 1");
                var opptyList = oppty.ToEnumerable(client).ToArray();
            });
        }

        [Fact]
        public async Task QueryRelationshipTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
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

        [Fact]
        public async Task CreateTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);

            await DeleteTestingRecords(client);

            try
            {
                var result1 = await client.CreateAsync("Case", new JObject
                {
                    ["Subject"] = $"UnitTest0"
                });
                result1.ThrowIfError();

                var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
                {
                    ["Subject"] = $"UnitTest{i}",
                }).ToArray();
                var result = await caseList.CreateAsync(client, true);
                result.ThrowIfError();
                Assert.NotEmpty(result.SuccessResponses());

                var expected = 200 * 26;
                var existingCase = await client.QueryAsync<JObject>($@"
SELECT Name FROM Opportunity ORDER BY Id LIMIT {expected}");

                var newCase = await existingCase.ToEnumerable(client)
                    .Select(c => new AttributedObject("Case") { ["Subject"] = $"UnitTest{c["Name"]}" })
                    .CreateAsync(client);
                newCase.ThrowIfError();
                Assert.NotEmpty(newCase.SuccessResponses());

                var obj = new AttributedObject("Account", $"acc{Guid.NewGuid():N}")
                {
                    ["Name"] = $"UnitTest Account {Guid.NewGuid():N}",
                    ["Contacts"] = new RecordsObject(Enumerable.Range(1, 2).Select(j =>
                         new AttributedObject("Contact", $"cont{Guid.NewGuid():N}")
                         { ["Name"] = $"UnitTest Contact{Guid.NewGuid():N}" }
                         ))
                };
                var createTreeResult = await client
                    .CreateAsync($"Account", new RecordsObject(new[] { obj }).ToCreateRequest());
                createTreeResult.ThrowIfError();
                Assert.NotEmpty(createTreeResult.Results);

                var accounts =
                    Enumerable.Range(1, 6).Select(i => new AttributedObject("Account", $"acc{i}")
                    {
                        ["Name"] = $"UnitTest Account {i}",
                        ["Contacts"] = new RecordsObject(Enumerable.Range(1, 2).Select(j =>
                            new AttributedObject("Contact", $"cont{i}[{j}]")
                            { ["Name"] = $"UnitTest Contact {i}-{j}" }))
                    });

                var accountsResult = await client.Composite.CreateTreeAsync("Account", accounts);
                accountsResult.ThrowIfError();
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
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
            await DeleteTestingRecords(client);

            try
            {
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var result = await client.CreateAsync($"Account", new JObject());
                    result.ThrowIfError();
                });
                await Assert.ThrowsAsync<ForceException>(async () =>
                {
                    var obj = new AttributedObject($"UnknownObject{Guid.NewGuid():N}");
                    var request = new CreateRequest { Records = new List<IAttributedObject> { obj } };
                    var result = await client.CreateAsync($"UnknownObject{Guid.NewGuid():N}", request);
                    result.ThrowIfError();
                });
                await Assert.ThrowsAsync<AggregateException>(async () =>
                {
                    var prd2List = Enumerable.Range(1, 4000).Select(i => new AttributedObject($"UnknownObject{Guid.NewGuid():N}"));
                    var result = await client.Composite.CreateAsync(prd2List);
                    result.ThrowIfError();
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
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
            //var externalIds = Enumerable.Range(1, 4000).Select(i => $"UnitTest{i}").ToArray();
            var result = await client.QueryAsync<JObject>($@"
SELECT Id FROM Product2 WHERE Source_Product_ID__c LIKE 'UnitTest%'");
            //var externalIds = Enumerable.Range(1, 4000).Select(i => new JObject { ["Source_Product_ID__c"] = $"UnitTest{i}" }).ToArray();
            Assert.NotEmpty(result.Records);
            //Assert.NotEmpty(result.Collections());
            //var ids = result.Objects().Values.Select(c => c["Id"].ToObject<string>()).ToArray();
            var result2 = await result.ToEnumerable(client).Select(c => c["Id"].ToObject<string>()).RetrieveAsync(client, "Product2", "Name");
            result2.ThrowIfError();
        }

        [Fact]
        public async Task UpdateTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
            //var externalIds = Enumerable.Range(1, 4000).Select(i => $"UnitTest{i}").ToArray();
            var result = (await client.QueryAsync<JObject>($@"
SELECT Id FROM Product2 WHERE Source_Product_ID__c LIKE 'UnitTest%'")).ToEnumerable(client);
            Assert.NotEmpty(result);
            var records = result.Select(c => c.Pick("Id").Assign(new AttributedObject("Product2") { ["Venue_ID__c"] = "V" })).ToArray();
            var result2 = await client.Composite.UpdateAsync(records);
            result2.ThrowIfError();
        }

        [Fact]
        public async Task UpdateEnumerableTest()
        {
            var expected = 100000;
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
            var oppty = await client.QueryAsync<JObject>($@"
SELECT Id FROM Opportunity ORDER BY Id LIMIT {expected}");
            var oppty2 = JToken.FromObject(oppty).ToObject<QueryResult<JObject>>();

            var timer1 = System.Diagnostics.Stopwatch.StartNew();
            var result = await oppty.ToEnumerable(client).UpdateAsync(client);
            timer1.Stop();

            Output.WriteLine($"time1: {timer1.Elapsed.TotalSeconds}.");
            WriteLine(result.ToString());
        }

        [Fact]
        public async Task DeleteTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);
            var result = await client.QueryAsync<JObject>($@"
SELECT Id FROM Product2 WHERE Source_Product_ID__c LIKE 'UnitTest%'");
            Assert.NotEmpty(result.Records);
            var result2 = await result.ToEnumerable(client).Select(r => r["Id"]?.ToObject<string>()).DeleteAsync(client);
            result2.ThrowIfError();
        }

        [Fact]
        public async Task CompositeTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);

            var uniqueId = $"{Guid.NewGuid():N}";

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

        [Fact]
        public async Task CompositeUrlInjectionTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);

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

        [Fact]
        public async Task CompositeSoqlInjectionTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);

            await DeleteTestingRecords(client);
            {
                var testText = $"UnitTest' AND Name != '";
                var request = new CompositeRequest(allOrNone: true);
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

                request.Delete("delete", "Product2", "@{created.Id}");
                var result = await client.Composite.PostAsync(request);
                result.ThrowIfError();
                Assert.Equal(testText, result.Results("created")?["Source_Product_ID__c"]);
                Assert.Equal(0, result.Queries("query")?.TotalSize);
                Assert.Equal(testText, result.Queries("upserted")?.Records?[0]?["Name"]);
                Assert.Equal(testText, result.Queries("updated")?.Records?[0]?["Name"]);
            }
            {
                var testText = $"UnitTest\\";
                var request = new CompositeRequest(allOrNone: true);
                request.Create("create", "Product2", GetTestProduct2().Assign(new JObject
                {
                    ["Source_Product_ID__c"] = testText,
                }));
                request.Retrieve("created", "Product2", "@{create.id}");

                // \ not working url injection
                //                request.Query("query", $@"
                //SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{{created.Id}}'
                //AND Source_Product_ID__c = '@{{created.Source_Product_ID__c}}n'");

                request.UpsertExternal("upsert", "Product2", "Source_Product_ID__c", "@{created.Source_Product_ID__c}", new JObject
                {
                    ["Name"] = testText,
                });
                request.Query("upserted", $@"
SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{{created.Id}}'
AND Name = {DNF.SOQLString(testText)}");

                //                request.Update("update", "Product2", new JObject
                //                {
                //                    ["Id"] = "@{created.Id}",
                //                    ["Source_Product_ID__c"] = "@{created.Source_Product_ID__c}\"",
                //                });
                //                request.Query("updated", $@"
                //SELECT Id, Name, Source_Product_ID__c FROM Product2 WHERE Id = '@{{created.Id}}'
                //AND Source_Product_ID__c = {DNF.SOQLString(testText + "\"")}");

                request.Delete("delete", "Product2", "@{created.Id}");
                var result = await client.Composite.PostAsync(request);
                result.ThrowIfError();
                Assert.Equal(testText, result.Results("created")?["Source_Product_ID__c"]);
                Assert.Equal(0, result.Queries("query")?.TotalSize);
                Assert.Equal(testText, result.Queries("upserted")?.Records?[0]?["Name"]);
                Assert.Equal(testText, result.Queries("updated")?.Records?[0]?["Name"]);
            }
        }

        [Fact]
        public async Task CompositeUrlParamInjectionTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);

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

        [Fact]
        public async Task CompositeJsonInjectionTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);

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
                request.Query("updated", @"
SELECT Id, Subject, SuppliedName FROM Case
WHERE Id = '@{create.id}'
AND Subject = 'UnitTest""'");
                request.Delete("delete", "Case", "@{updated.records[0].Id}");
                var result = await client.Composite.PostAsync(request);
                result.ThrowIfError();
            }
        }

        [Fact]
        public async Task CompositeFailTest()
        {
            var client = await DNFClient.LoginAsync(LoginUri, ClientId, ClientSecret, UserName, Password, WriteLine);

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
