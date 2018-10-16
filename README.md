# DotNetForce

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
var record = (await client.QueryAsync<JObject>($@"
SELECT ID FROM Opportunity WHERE Name = {DNF.SOQLString(name)}" LIMIT 1")).Records.FirstOrDefault();
```

Query By Id
```cs
var oppId = "006xxxx";
var record = await client.QueryByIdAsync<JObject>("Opportunity", oppId);
```

Query 
```cs
var result = await client.QueryAsync<JObject>($@"
SELECT Id FROM Opportunity WHERE LastModifiedDate > {DNF.SOQLDateTime(new DateTime(2018, 1, 1))}");
var records = result.ToEnumerable(client);
```

Query (included deleted)
```cs
var result = await client.QueryAllAsync<JObject>($@"
SELECT Id FROM Opportunity WHERE LastModifiedDate > {DNF.SOQLDateTime(new DateTime(2018, 1, 1))}");
var records = result.ToEnumerable(client);
```

Query Sub-query
```cs
var result = await client.QueryAsync<JObject>($@"
SELECT Id, (SELECT Id FROM Opportunities) FROM Account LIMIT 1");
for (var acc in result.Records) {
   var records = result.ToEnumerable(acc, "Opportunities");
}
```

### Create

Create a record
```cs
var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
{
  ["Subject"] = $"UnitTest{i}",
}).ToArray();
var result = await caseList.CreateAsync(client, true);
result.ThrowIfError();
Assert.NotEmpty(result.SuccessResponses());
```

 Create mutlipls records (in transaction, allOrNone: true, up to 25 records can be created)
```cs
 var caseList = Enumerable.Range(1, 40).Select(i => new AttributedObject("Case")
 {
     ["Subject"] = $"UnitTest{i}",
 }).ToArray();
 var result = await caseList.CreateAsync(client, true);
 result.ThrowIfError();
 Assert.NotEmpty(result.SuccessResponses());
 ```
 
 Create mutlipls records (no transaction, no limit)
```cs
 var expected = 200 * 26;
 var existingCase = await client.QueryAsync<JObject>($@"
SELECT Name FROM Opportunity ORDER BY Id LIMIT {expected}");

 var newCase = await existingCase.ToEnumerable(client)
     .Select(c => new AttributedObject("Case") { ["Subject"] = $"UnitTest{c["Name"]}" })
     .CreateAsync(client);
 newCase.ThrowIfError();
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
accountsResult.ThrowIfError();
Assert.NotEmpty(accountsResult.Results);
```

### Retreive

Retreive a retreive by Id

oops, still under contruction, will continue when i have the time...
 

### Code sample
See. https://github.com/ste80/DotNetForce/tree/master/DotNetForceTest
Refer to Unit Test for code sample.


