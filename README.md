# DotNetForce


V3.0.0

I tried put DotNetForce.Schema on nuget, but it doesn't work.


following is the step by step guid to generator Schema for your org

1) Download all the files from (Except DotNetForce.Schema.csproj) https://github.com/ste80/DotNetForce/tree/master/DotNetForce.Schema

2) Place the files in the root of your project

3) Open DEV.tt and update the your_app_client_id, your_app_client_secret, your_app_redirect_uri_for_server_flow_only, your_user_name_for_password_flow_only, your_password_for_password_flow_only

4) Run the DEV.tt using T4

5) a new folder DEV should be created which contains the Schema helper for your org.

I have updated the library, but don't have time to document the changes, please check the TestClass


V2.0.7
1) increase query string limit to 20000 characters
2) new Client.GetEnumerableByIdsAsync
usages:
```cs
var opp = Schema.Of(s => s.Opportunity);
var lotsofIds = new [] { "0060I00000QkwK5"... x 100000 };

var results = (await Client.GetEnumerableByIdsAsync(lotsofIds, $@"
SELECT {line.Opportunity.AccountId}
FROM {opp}
WHERE {opp.Id} IN(<ids>)", "<ids>")).ToList();
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
var oppties = await Schema.Wrap(Client.GetEnumerableAsync($@"
SELECT {opp.Id}, {opp.Account.CreatedBy.Name}, (SELECT {line.ListPrice} FROM {opp.OpportunityLineItems})
FROM {opp}
WHERE {opp.Name} LIKE 'Test'
"));
foreach (var oppObj in oppties)
{
    var id = oppObj.Get(opp.Id);
    var createdByName = oppObj.Get(opp.CreatedBy.Name);
    var oppLines = Schema.Wrap(Client.GetEnumerable(oppObj.Get(opp.OpportunityLineItems)));
    foreach (var oppLine in oppLines)
    {
        var listPrice = oppLine.Get(line.ListPrice);
    }
}
```

I plan to create this libaray for TypeScript later.

I plan to create this libaray for TypeScript later.
--------------------------------------------------------------------------------

Original repository https://github.com/developerforce/Force.com-Toolkit-for-NET

Currently it support the following

1) Rest API, when query large dataset, after the first batch is retrieved (usually 2000 records will be returned for first batch)
   it will put 5 next queries in 1 composite API call as a chunk and run all the chunks in parallel (not lazy loading)
2) Composite, Multiple request in 1 api call
   (when allOrNone: true, up to 25 requests, 5 of them can bee soql query).
   (when allOrNone: false, there no limit, the requests will be ran in parallel).
   https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_composite.htm
3) Composite Tree, create up to 100 objects and its related child objects in 1 api call
   https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_sobject_tree.htm
4) Composite Collection, up to 2000 x 5 retrieve or 200 x 5 create|update|delete requests in 1 api call
   x 5 because I can put 5 composite collection requests in 1 Composite API call
   https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/resources_composite_sobjects_collections.htm
5) Batch, Executes up to 25 subrequests in a single request. unlike the above Composite API
   Each subrequest counts against rate limits.
6) Tooling API, let you run apex using rest api etc.
   https://developer.salesforce.com/docs/atlas.en-us.api_tooling.meta/api_tooling/intro_api_tooling.htm
7) Chatter API same as Force.com-Toolkit-for-NET
   https://developer.salesforce.com/docs/atlas.en-us.chatterapi.meta/chatterapi/
8) Bulk API same as Force.com-Toolkit-for-NET
   https://developer.salesforce.com/docs/atlas.en-us.api_asynch.meta/api_asynch/asynch_api_intro.htm

This library is now targeting .NET Standard 2.0.

### Authentication

To access the Force.com APIs you must have a valid Access Token. Currently there are two ways to generate an Access Token: the [Username-Password Authentication Flow](http://help.salesforce.com/HTViewHelpDoc?id=remoteaccess_oauth_username_password_flow.htm&language=en_US) and the [Web Server Authentication Flow](http://help.salesforce.com/apex/HTViewHelpDoc?id=remoteaccess_oauth_web_server_flow.htm&language=en_US)

#### Username-Password Authentication Flow

The Username-Password Authentication Flow is a straightforward way to get an access token. Simply provide your consumer key, consumer secret, username, and password.

```cs
var client = await DNFClient.LoginAsync(new Uri("https://www.salesforce.com"), "YOUR_CLIENT_ID", "YOUR_CLIENT_SECRET", "YOUR_USER_NAME", "YOUR_PASSWORD");
```

#### Web-Server Authentication Flow

The Web-Server Authentication Flow requires a few additional steps but has the advantage of allowing you to authenticate your users and let them interact with the Force.com using their own access token.

First, you need to authenticate your user. You can do this by creating a URL that directs the user to the Salesforce authentication service. You'll pass along some key information, including your consumer key (which identifies your Connected App) and a callback URL to your service.

After the user logs in you'll need to handle the callback and retrieve the code that is returned. Using this code, you can then request an access token.

```cs
var client await DNFClient.LoginAsync(new OAuthProfile
{
    LoginUri = new Uri("https://www.salesforce.com),
    ClientId ="YOUR_CLIENT_ID",
    ClientSecret = "YOUR_CLIENT_SECRET",
    Code = "YOUR_ACCESS_TOKEN_FROM_AUTH_SERVICE",
    RedirectUri = "YOUR_REDIRECT_URI",
    RefreshToken = "YOUR_REFRESH_TOKEN"
});
```

For refreshToken, you need refresh_token "permission", even you already have "full" permission.
https://developer.salesforce.com/docs/atlas.en-us.packagingGuide.meta/packagingGuide/connected_app_create.htm

```cs
var client await DNFClient.LoginAsync(new OAuthProfile
{
    LoginUri = new Uri("https://www.salesforce.com),
    ClientId ="YOUR_CLIENT_ID",
    ClientSecret = "YOUR_CLIENT_SECRET",
    Code = "YOUR_ACCESS_TOKEN_FROM_AUTH_SERVICE",
    RedirectUri = "YOUR_REDIRECT_URI",
    RefreshToken = "YOUR_REFRESH_TOKEN"
});


.... to refresh call this
await client.TokenRefreshAsync();
```

### Query

Query 1 records
```cs
var name = "abc";
var record = (await client.QueryAsync($@"
SELECT ID FROM Opportunity WHERE Name = {DNF.SOQLString(name)}" LIMIT 1")).Records.FirstOrDefault();
```

Query By Id
```cs
var oppId = "006xxxx";
var record = await client.QueryByIdAsync("Opportunity", oppId);
```

Query 
```cs
var records = await client.GetEnumerableAsync($@"
SELECT Id FROM Opportunity WHERE LastModifiedDate > {DNF.SOQLDateTime(new DateTime(2018, 1, 1))}");
```

Query (included deleted)
```cs
var result = await client.GetAllEnumerableAsync($@"
SELECT Id FROM Opportunity WHERE LastModifiedDate > {DNF.SOQLDateTime(new DateTime(2018, 1, 1))}");
var records = result.GetEnumerable(client);
```

Query Sub-query
```cs
var result = await client.GetAllEnumerableAsync($@"
SELECT Id, (SELECT Id FROM Opportunities) FROM Account LIMIT 1");
for (var acc in result) {
   var records = client.GetEnumerable((QueryResult<JObject>)acc["Opportunities"]);
}
```

### Create

Create a record
```cs
var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
{
  ["Subject"] = $"UnitTest{i}",
}).ToArray();
var result = await client.Composite.CreateAsync(caseList, true);
DNF.ThrowIfError(result);
Assert.NotEmpty(result.SuccessResponses());
```

 Create multiple records (in transaction, allOrNone: true, up to 25 records can be created)
```cs
 var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
 {
     ["Subject"] = $"UnitTest{i}",
 }).ToArray();
 var result = await client.Composite.CreateAsync(caseList, true);
 DNF.ThrowIfError(result);
 Assert.NotEmpty(result.SuccessResponses());
 ```
 
 Create multiple records (no transaction, no limit)
```cs
 var expected = 200 * 26;
 var existingCase = await client.GetEnumerableAsync($@"
SELECT Name FROM Opportunity ORDER BY Id LIMIT {expected}");

 var newCase = await existingCase
     .Select(c => new AttributedObject("Case") { ["Subject"] = $"UnitTest{c["Name"]}" })
     .CreateAsync(client);
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

var accountsResult = await client.Composite.CreateTreeAsync("Account", accounts);
DNF.ThrowIfError(accountsResult);
Assert.NotEmpty(accountsResult.Results);
```

### Retreive

Retreive a retreive by Id

oops, still under contruction, will continue when i have the time...
 

### Code sample
See. https://github.com/ste80/DotNetForce/tree/master/DotNetForceTest
Refer to Unit Test for code sample.


