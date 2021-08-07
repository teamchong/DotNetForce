# DotNetForce

nuget package <https://www.nuget.org/packages/DotNetForce>

Run this on nuget command to fix the binding problem

```nuget
Get-Project –All | Add-BindingRedirect
```

Please check the TestClasses for code samples <https://github.com/teamchong/DotNetForce/tree/master/DotNetForceTest>

This library provides an easy way for .NET developers to interact with the Salesforce REST & Chatter APIs using native libraries.

v6.0.0

Rewrote to support netstandard 2.1

v5.0.9

new function GetAsyncEnumerableByFieldValues, support any string field

Add test class for GetAsyncEnumerableByIds, GetAsyncEnumerableByFieldValues

<https://github.com/teamchong/DotNetForce/blob/master/DotNetForceTest/DNFClientTest.cs>

v5.0.8

Bug Fixed JObjectWrapper.Set support relationship field

v5.0.7

Remove ExplainAllAsync

BugFixed: QueryAll

(All = include deleted & achieved)

V5.0.1

Add support for Blazor (WebAssembly)

+Setup <https://teamchong.github.io/DotNetForce/> for demo running C# on web client

+DNFClient.DefaultApiVersion = "v52.0"; // to change API Version

+DNFClient.UseCompression = false; // to disabled UseCompression

+DNFClient.Proxy = uri => new Uri(new Uri("https://yourproxy"), uri.PathAndQuery); // to send to proxy instead of instanceUrl

V4.0.0

I tried put DotNetForce.Schema on nuget, but it doesn't work.

following is the step by step guide to generate Schema for your org

1) Install the latest version from nuget (v4.0.0+) <https://nuget.org/packages/DotNetForce>

2) Download all the files from (Except DotNetForce.Schema.csproj) <https://github.com/teamchong/DotNetForce/tree/master/DotNetForce.Schema>

3) Place the files in the root of your project

4) Open DEV.tt and update the your_app_client_id, your_app_client_secret, your_app_redirect_uri_for_server_flow_only, your_user_name_for_password_flow_only, your_password_for_password_flow_only <https://raw.githubusercontent.com/teamchong/DotNetForce/master/DotNetForce.Schema/DEV.tt>

5) Run the DEV.tt using T4

6) a new DEV.cs should be created which contains the Schema helper classes for your org.

V2.0.7

1) increase query string limit to 20000 characters
2) new Client.GetAsyncEnumerableByIds
usages:

```cs
var (opp, acc) = Schema.Of(s => (s.Opportunity, s.Account));
var lotsOfOpps = await Schema.Wrap(client.GetAsyncEnumerable($@"
SELECT {opp.AccountId}
FROM {opp}
LIMIT 100000")).ToListAsync().ConfigureAwait(false);
var lotsofIds = lotsOfOpps.Select(o => o.Get(opp.AccountId)).ToList(); // new [] { "0010I00000QkwK5"... x 100000 };

var results = Schema.Wrap(Client.GetAsyncEnumerableByIds(lotsofIds, $@"
SELECT {acc.Id}, {acc.AccountNumber}, {acc.Owner.Email}
FROM {acc}
WHERE {acc.Id} IN(<ids>)", "<ids>")).ToListAsync().ConfigureAwait(false); // <ids> is a text template you defined, you have multiple template, it willl be replaced by '0010I00000QkwK5','0010I00000QkwK5' etc.
```

V2.0.0

1) new "DotNetForce.Schema" created by T4 template, please fill in LoginProfiles.json, and run the T4 template "DEV.tt" using Visual Studio

usages

```cs
var opp = Schema.Of(s => s.Opportunity);
var line = Schema.Of(s => s.OpportunityLineItem);
```

or

```cs
var (opp, line) = Schema.Of(s => (s.Opportunity, s.OpportunityLineItem));
```

```cs
var oppties = Schema.Wrap(Client.GetAsyncEnumerable($@"
SELECT {opp.Id}, {opp.Account.CreatedBy.Name}, (SELECT {line.ListPrice} FROM {opp.OpportunityLineItems})
FROM {opp}
WHERE {opp.Name} LIKE 'Test'
"));
await foreach (var oppObj in oppties)
{
    var id = oppObj.Get(opp.Id);
    var createdByName = oppObj.Get(opp.CreatedBy.Name);
    var oppLines = Schema.Wrap(Client.GetAsyncEnumerable(oppObj.Get(opp.OpportunityLineItems)));
    await foreach (var oppLine in oppLines)
    {
        var listPrice = oppLine.Get(line.ListPrice);
    }
}
```

--------------------------------------------------------------------------------

Original repository <https://github.com/developerforce/Force.com-Toolkit-for-NET>

Currently it support the following

1) Rest API, when query large dataset, after the first batch is retrieved (usually 2000 records will be returned for first batch)
   it will put 5 next queries in 1 composite API call as a chunk and run all the chunks in parallel (not lazy loading)
2) Composite, Multiple request in 1 api call
   (when allOrNone: true, up to 25 requests, 5 of them can bee soql query).
   (when allOrNone: false, there no limit, the requests will be ran in parallel).
   <https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_composite.htm>
3) Composite Tree, create up to 100 objects and its related child objects in 1 api call
   <https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_sobject_tree.htm>
4) Composite Collection, up to 2000 x 5 retrieve or 200 x 5 create|update|delete requests in 1 api call
   x 5 because I can put 5 composite collection requests in 1 Composite API call
   <https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_sobjects_collections.htm>
5) Batch, Executes up to 25 subrequests in a single request. unlike the above Composite API
   Each subrequest counts against rate limits.
6) Tooling API, let you run apex using rest api etc.
   <https://developer.salesforce.com/docs/atlas.en-us.api_tooling.meta/api_tooling/intro_api_tooling.htm>
7) Chatter API same as Force.com-Toolkit-for-NET
   <https://developer.salesforce.com/docs/atlas.en-us.chatterapi.meta/chatterapi/>
8) Bulk API same as Force.com-Toolkit-for-NET
   <https://developer.salesforce.com/docs/atlas.en-us.api_asynch.meta/api_asynch/asynch_api_intro.htm>

This library is now targeting .NET Standard 2.0.

## Authentication

To access the Force.com APIs you must have a valid Access Token. Currently there are two ways to generate an Access Token: the [Username-Password Authentication Flow](http://help.salesforce.com/HTViewHelpDoc?id=remoteaccess_oauth_username_password_flow.htm&language=en_US) and the [Web Server Authentication Flow](http://help.salesforce.com/apex/HTViewHelpDoc?id=remoteaccess_oauth_web_server_flow.htm&language=en_US)

### Username-Password Authentication Flow

The Username-Password Authentication Flow is a straightforward way to get an access token. Simply provide your consumer key, consumer secret, username, and password.

```cs
var client = await DNFClient.LoginAsync(new Uri("https://www.salesforce.com"), "YOUR_CLIENT_ID", "YOUR_CLIENT_SECRET", "YOUR_USER_NAME", "YOUR_PASSWORD").ConfigureAwait(false);
```

### Web-Server Authentication Flow

The Web-Server Authentication Flow requires a few additional steps but has the advantage of allowing you to authenticate your users and let them interact with the Force.com using their own access token.

First, you need to authenticate your user. You can do this by creating a URL that directs the user to the Salesforce authentication service. You'll pass along some key information, including your consumer key (which identifies your Connected App) and a callback URL to your service.

After the user logs in you'll need to handle the callback and retrieve the code that is returned. Using this code, you can then request an access token.

```cs
var client await DNFClient.LoginAsync(new OAuthProfile
{
    LoginUri = new Uri("https://www.salesforce.com"),
    ClientId ="YOUR_CLIENT_ID",
    ClientSecret = "YOUR_CLIENT_SECRET",
    Code = "YOUR_ACCESS_TOKEN_FROM_AUTH_SERVICE",
    RedirectUri = "YOUR_REDIRECT_URI",
    RefreshToken = "YOUR_REFRESH_TOKEN"
}).ConfigureAwait(false);
```

For refreshToken, you need refresh_token "permission", even you already have "full" permission.
<https://developer.salesforce.com/docs/atlas.en-us.packagingGuide.meta/packagingGuide/connected_app_create.htm>

```cs
var client await DNFClient.LoginAsync(new OAuthProfile
{
    LoginUri = new Uri("https://www.salesforce.com"),
    ClientId ="YOUR_CLIENT_ID",
    ClientSecret = "YOUR_CLIENT_SECRET",
    Code = "YOUR_ACCESS_TOKEN_FROM_AUTH_SERVICE",
    RedirectUri = "YOUR_REDIRECT_URI",
    RefreshToken = "YOUR_REFRESH_TOKEN"
}).ConfigureAwait(false);


.... to refresh call this
await client.TokenRefreshAsync().ConfigureAwait(false);
```

## Query

Query 1 records

```cs
var name = "abc";
var record = (await client.QueryAsync($@"
SELECT ID FROM Opportunity WHERE Name = {DNF.SOQLString(name)} LIMIT 1").ConfigureAwait(false)
).Records.FirstOrDefault();
```

Query By Id

```cs
var oppId = "006xxxx";
var record = await client.QueryByIdAsync("Opportunity", oppId).ConfigureAwait(false);
```

Query

```cs
var records = await client.GetAsyncEnumerable($@"
SELECT Id FROM Opportunity WHERE LastModifiedDate > {DNF.SOQLDateTime(new DateTime(2018, 1, 1))}")
    .ToListAsync().ConfigureAwait(false);
```

Query (included deleted)

```cs
var result = await client.GetAsyncEnumerableAll($@"
SELECT Id FROM Opportunity WHERE LastModifiedDate > {DNF.SOQLDateTime(new DateTime(2018, 1, 1))}")
    .ToListAsync().ConfigureAwait(false);
var records = await result.GetAsyncEnumerable(client).ToListAsync().ConfigureAwait(false);
```

Query Sub-query

```cs
var result = await client.GetAsyncEnumerableAllAsync($@"
SELECT Id, (SELECT Id FROM Opportunities) FROM Account LIMIT 1").ToListAsync().ConfigureAwait(false);
for (var acc in result) {
   var records = client.GetAsyncEnumerable((QueryResult<JObject>)acc["Opportunities"]);
}
```

## Create

Create a record

```cs
var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
{
  ["Subject"] = $"UnitTest{i}",
}).ToArray();
var result = await client.Composite.CreateAsync(caseList, true).ConfigureAwait(false);
DNF.ThrowIfError(result);
Assert.NotEmpty(result.SuccessResponses());
```

 Create multiple records (in transaction, allOrNone: true, up to 25 records can be created)

```cs
 var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
 {
     ["Subject"] = $"UnitTest{i}",
 }).ToArray();
 var result = await client.Composite.CreateAsync(caseList, true).ConfigureAwait(false);
 DNF.ThrowIfError(result);
 Assert.NotEmpty(result.SuccessResponses());
 ```

 Create multiple records (no transaction, no limit)

```cs
 var expected = 200 * 26;
 var existingCase = await client.GetAsyncEnumerable($@"
SELECT Name FROM Opportunity ORDER BY Id LIMIT {expected}");

 var newCase = await existingCase
     .Select(c => new AttributedObject("Case") { ["Subject"] = $"UnitTest{c["Name"]}" })
     .CreateAsync(client).ConfigureAwait(false);
 DNF.ThrowIfError(newCase);
 Assert.NotEmpty(newCase.SuccessResponses());
 ```

Create Tree

```cs
var accounts =
   Enumerable.Range(1, 6).Select(i => new AttributedObject("Account", $"acc{i}")
   {
      ["Name"] = $"UnitTest Account {i}",
      ["Contacts"] = new RecordsObject(Enumerable.Range(1, 2).Select(j =>
          new AttributedObject("Contact", $"cont{i}[{j}]")
          { ["Name"] = $"UnitTest Contact {i}-{j}" }))
   });

var accountsResult = await client.Composite.CreateTreeAsync("Account", accounts).ConfigureAwait(false);
DNF.ThrowIfError(accountsResult);
Assert.NotEmpty(accountsResult.Results);
```

## Retrieve

Retrieve a retreive by Id

oops, still under contruction, will continue when i have the time...

## Code sample

See. <https://github.com/ste80/DotNetForce/tree/master/DotNetForceTest>
Refer to Unit Test for code sample.
