# csharp-bandwidth-extras

[![nuget version](https://badge.fury.io/nu/Bandwidth.Net.Extra.svg)](https://badge.fury.io/nu/Bandwidth.Net.Extra)
[![Build Status](https://travis-ci.org/Bandwidth/csharp-bandwidth-extras.svg?branch=master)](https://travis-ci.org/Bandwidth/csharp-bandwidth-extras)

Helper functions and ASP.Net Core middleware for Bandwidth.Net. Read more documentation [here](http://dev.bandwidth.com/csharp-bandwidth-extras/api/index.html).

## Install

Run

```
dotnet add package Bandwidth.Net.Extra
```

## Examples

### Helpers

```csharp
using Bandwidth.Netx.Extra;

var appId = await client.Application.GetOrCreateAsync('My app', 'my.domain.com'); // It will return exisitng application Id or create it otherwise

var number = await client.PhoneNumber.GetOrCreateAsync(client.AvailbaleNumber, appId, 'Support', new LocalNumberQueryForOrder{AreaCode = "910"}); // It will reserve a linked to this app phone number and assign name to it. If number with such name already exists it returns it.
```

### Middleware

#### Koa

```csharp
public class Startup
{
   public void ConfigureServices(IServiceCollection services)
   {
      // Register Banwidth API services. They are used by middleware too.
      services.AddBandwidth(new BandwidthAuthData
      {
        UserId = "userId",
        ApiToken = "apiToken",
        ApiSecret = "apiSecret"
      });
   }
   public void Configure(IApplicationBuilder app)
   {
      // Using a middleware
      app.UseBandwidth(new BandidthOptions
      {
        ApplicationName = "My App",
        PhoneNumber = new PhoneNumberOptions
        {
          LocalNumberQueryForOrder = new LocalNumberQueryForOrder // leave LocalNumberQueryForOrder with null to allocate toll free number
          {
            AreaCode = "910"
          }
        },
        CallCallback = async (eventData, context) // as alternative you can use CallCallbackDictionary to handle only specific events
        {
          // handle call events here (including sip events too)
          if(eventData.EventType == CallbackEventType.Answer && context.Items["PhoneNumber"] == eventData.To){
            Debug.WriteLine('Answered');
          }
        }
      });
   }
}
```

Now in you contollers you can use any `Bandwidth.Net` interface and `Client` instance via DI. Also `HttpContext.Items["ApplicationId"]` will return application id on Bandwidth server, `HttpContext.Items["PhoneNumber"]` will return allocated phone number.

```csharp
public class MyController: Controller
{
  public MyController(ICall call) // via Dependency Injection
  {

  }

  public IActionResult MyAction()
  {
    return Json(new {PhoneNumber = HttpContext.Items["PhoneNumber"]}); // using allocated phone number
  }
}
```
