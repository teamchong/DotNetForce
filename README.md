# DotNetForce

Original repository https://github.com/developerforce/Force.com-Toolkit-for-NET

Currently it support the following

1) Rest API, when query large dataset, after the first batch is retrieved
   it will put 5 NextQueryUrls in 1 composite API call as a chunk, run all the chunks in parallel (not lazy loading)
2) Composite, Multiple request in 1 api call
   (when allOrNone: true, up to 25 requests, 5 of them can soql query).
   (when allOrNone: false, there no limit, the requests will be ran in parallel).
3) Composite Tree, create up to 100 objects and its related child objects in 1 api call
4) Compoiste Collection, up to 2000 x 5 retrieve or 200 x 5 create|update|delete requests in 1 api call
5) Tooling API, let you run apex using rest api etc.
   https://developer.salesforce.com/docs/atlas.en-us.api_tooling.meta/api_tooling/intro_api_tooling.htm
6) Chatter API same as Force.com-Toolkit-for-NET
   https://developer.salesforce.com/docs/atlas.en-us.chatterapi.meta/chatterapi/
7) Bulk API same as Force.com-Toolkit-for-NET
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
});
```

```
var bulkClient = new BulkForceClient(instanceUrl, accessToken, apiVersion);
```

### Sample Code

under construction....
