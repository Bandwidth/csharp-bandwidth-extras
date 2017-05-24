using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Bandwidth.Net;
using Bandwidth.Net.Api;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bandwidth.Net.Extra
{
  /// <summary>
  /// Extensions for ASP.Net Core
  /// </summary>
  public static class AspNetCoreExtensions 
  {
    /// <summary>
    /// Bandwidth middleware for ASP.Net Core
    /// </summary>
    /// <param name="builder">IApplicationBuilder instance</param>
    /// <param name="options">Options of middleware</param>
    /// <returns>IApplicationBuilder instance</returns>
    /// <remarks>
    /// It links your web application with application on Bandwidth server. It defines routes for event callbacks and allows to add custom handlers easy.!--
    /// Also it allocate a phone number which events will be handled by this application. It receives domain Id (and creates it if need) for your SIP domain if need.
    /// 
    /// Don't forget run <code>services.AddBandwidth(new BandwidthAuthData(...))</code> before use this middleware.
    /// </remarks>
    /// <example>
    /// Simple demo
    /// <code>
    /// public class Startup
    /// {
    ///    public void Configure(IApplicationBuilder app)
    ///    {
    ///       app.UseBandwidth(new BandidthOptions
    ///       {
    ///         ApplicationName = "My App",
    ///         PhoneNumber = new PhoneNumberOptions
    ///         {
    ///           LocalNumberQueryForOrder = new LocalNumberQueryForOrder
    ///           {
    ///             AreaCode = "910"
    ///           }
    ///         },
    ///         CallCallback = async (eventData, context)
    ///         {
    ///           // handle calls event here
    ///         }
    ///       });
    ///    }
    /// }
    /// </code>
    /// Now you can use in any route handler: 
    /// <code>Context.items["PhoneNumber"]</code> - allocated phone number which events will be handled by this app
    /// <code>Context.items["ApplicationId"]</code> - application Id of Bandwidth application
    /// </example>
    /// <example>
    /// Demo with SIP domain
    /// <code>
    /// public class Startup
    /// {
    ///    public void ConfigureServices(IServiceCollection services)
    ///    {
    ///       // Register Banwidth API services. They are used by middleware.
    ///       services.AddBandwidth(new BandwidthAuthData
    ///       {
    ///         UserId = "userId",
    ///         ApiToken = "apiToken",
    ///         ApiSecret = "apiSecret"
    ///       });
    ///    }
    ///    public void Configure(IApplicationBuilder app)
    ///    {
    ///       // Using a middleware
    ///       app.UseBandwidth(new BandidthOptions
    ///       {
    ///         ApplicationName = "My App",
    ///         PhoneNumber = new PhoneNumberOptions
    ///         {
    ///           LocalNumberQueryForOrder = new LocalNumberQueryForOrder
    ///           {
    ///             AreaCode = "910"
    ///           }
    ///         },
    ///         DomainName = "my-domain",
    ///         CallCallback = async (eventData, context)
    ///         {
    ///           // handle call events here (including sip events too)
    ///         }
    ///       });
    ///    }
    /// }
    /// </code>
    /// Now you can use in any route handler: 
    /// <code>Context.items["PhoneNumber"]</code> - allocated phone number which events will be handled by this app
    /// <code>Context.items["ApplicationId"]</code> - application Id of Bandwidth application
    /// <code>Context.items["DomainId"]</code> - domain Id of your SIP domain on Banwidth server. You can use it to manage sip endpoints.
    /// </example>
    /// <example>
    /// Event handler as dictionary
    /// <code>
    /// public class Startup
    /// {
    ///    public void Configure(IApplicationBuilder app)
    ///    {
    ///       app.UseBandwidth(new BandidthOptions
    ///       {
    ///         ApplicationName = "My App",
    ///         CallCallbackDictionary = new Dictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>>
    ///         {
    ///           {CallbackEvent.Answer, async (eventData, context)
    ///           {
    ///             // handle event "answer" here
    ///           }}
    ///         }
    ///       });
    ///    }
    /// }
    /// </code>
    /// </example>
    public static IApplicationBuilder UseBandwidth(this IApplicationBuilder builder, BandwidthOptions options)
    {
      builder.Use(async (context, next) => {
        var memoryCache = context.RequestServices.GetService<IMemoryCache>();
        var application = context.RequestServices.GetService<IApplication>();
        var phoneNumber = context.RequestServices.GetService<IPhoneNumber>();
        var availableNumber = context.RequestServices.GetService<IAvailableNumber>();
        var domain = context.RequestServices.GetService<IDomain>();
        
        var applicationId = await memoryCache.CachedCall($"{options.ApplicationName}##{context.Request.Host.Host}", 
          () => application.GetOrCreateAsync(options.ApplicationName, context.Request.Host.Host, context.Request.IsHttps, context.RequestAborted));
        context.Items["ApplicationId"] = applicationId;
        if (options.PhoneNumber != null)
        {
          if (options.PhoneNumber.LocalNumberQueryForOrder != null)
          {
            context.Items["PhoneNumber"] = await memoryCache.CachedCall(applicationId, 
              () => phoneNumber.GetOrCreateLocalAsync(availableNumber, applicationId, options.PhoneNumber.LocalNumberQueryForOrder, options.PhoneNumber.Name, context.RequestAborted));
          }
          else
          {
            context.Items["PhoneNumber"] = await memoryCache.CachedCall(applicationId, 
              () => phoneNumber.GetOrCreateTollFreeAsync(availableNumber, applicationId, options.PhoneNumber.Name, context.RequestAborted));
          }
        }
        if (!string.IsNullOrEmpty(options.DomainName))
        {
          context.Items["DomainId"] = await memoryCache.CachedCall(options.DomainName, 
            () => domain.GetOrCreateAsync(options.DomainName));
        }
        await next();
      });
      builder.AddRouteHandler(ApplicationExtensions.CallCallbackPath, options.CallCallback);
      builder.AddRouteHandler(ApplicationExtensions.MessageCallbackPath, options.MessageCallback);
      return builder;
    }

    private static void AddRouteHandler(this IApplicationBuilder builder, string route, Func<CallbackEvent, HttpContext, Task> handler) 
    {
      if(handler == null)
      {
        return;
      }
      builder.Map(route, b => {
        b.Use(async (context, next) => {
          if (context.Request.Method != HttpMethods.Post || !(context.Request.ContentType ?? "").Contains("/json"))
          {
            await next();
            return;
          }
          var logger = context.RequestServices.GetService<ILogger>();
          try
          {
            using(var streamReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8))
            {
              var json = await streamReader.ReadToEndAsync();
              logger?.LogDebug("BandwidthEventCallback", json);
              var eventData = CallbackEvent.CreateFromJson(json);
              await handler(eventData, context);
            }
          }
          catch (Exception ex)
          {
           logger?.LogError("BandwidthEventCallback", ex); 
          }
          await context.Response.WriteAsync("");
        });
      });
    }

    private static Task<T> CachedCall<T>(this IMemoryCache cache, string key, Func<Task<T>> func)
    {
      return cache.GetOrCreateAsync(key, e => {
        e.Priority = CacheItemPriority.NeverRemove;
        return func();
      });
    }

    /// <summary>
    /// DI helper for Bandwidth
    /// </summary>
    /// <param name="services">IServiceCollection instance</param>
    /// <param name="authData">Banswisth auth data</param>
    /// <returns>IServiceCollection instance</returns>
    /// <remarks>
    /// It registers interfaces and Client instance of Bandwidth.Net into service collection. After that you can use any such type via DI in your controllers.
    /// </remarks>
    /// <example>
    /// Event handler as dictionary
    /// <code>
    /// public class Startup
    /// {
    ///    public void ConfigureServices(IServiceCollection services)
    ///    {
    ///       services.AddBandwidth(new BandwidthAuthData
    ///       {
    ///         UserId = "userId",
    ///         ApiToken = "apiToken",
    ///         ApiSecret = "apiSecret"
    ///       });
    ///    }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddBandwidth(this IServiceCollection services, BandwidthAuthData authData)
    {
      var client = new Client(authData.UserId, authData.ApiToken, authData.ApiSecret);
      services.AddSingleton(client);
      services.AddSingleton(client.Account);
      services.AddSingleton(client.Application);
      services.AddSingleton(client.AvailableNumber);
      services.AddSingleton(client.Bridge);
      services.AddSingleton(client.Call);
      services.AddSingleton(client.Conference);
      services.AddSingleton(client.Domain);
      services.AddSingleton(client.Endpoint);
      services.AddSingleton(client.Error);
      services.AddSingleton(client.Media);
      services.AddSingleton(client.Message);
      services.AddSingleton(client.NumberInfo);
      services.AddSingleton(client.PhoneNumber);
      services.AddSingleton(client.Recording);
      services.AddSingleton(client.Transcription);
      services.AddSingleton(client.V2.Message);
      services.AddMemoryCache();
      return services;
    }
  }

  /// <summary>
  /// Bandwidth auth data
  /// </summary>
  public class BandwidthAuthData
  {
    /// <summary>
    /// UserId on Bandwidth server
    /// </summary>
    public string UserId {get; set;}

    /// <summary>
    /// Api token on Bandwidth server
    /// </summary>
    public string ApiToken {get; set;}

    /// <summary>
    /// Api secret on Bandwidth server
    /// </summary>
    public string ApiSecret {get; set;}
  }

  /// <summary>
  /// Middleware options
  /// </summary>
  public class BandwidthOptions
  {
    /// <summary>
    /// Application name on Bandwidth server
    /// </summary>
    public string ApplicationName {get; set;}

    /// <summary>
    /// Options to allocate phoen number. If missing no phone number will be allocated.
    /// </summary>
    public PhoneNumberOptions PhoneNumber {get; set;}

    /// <summary>
    /// Domain name for SIP endpoints. If missing no domain Id will be retrived.
    /// </summary>
    /// <returns></returns>
    public string DomainName {get; set;}

    /// <summary>
    /// Handler of call callback events
    /// </summary>
    public Func<CallbackEvent, HttpContext, Task> CallCallback {get; set;}
    
    /// <summary>
    /// Handler of message callback events
    /// </summary>
    public Func<CallbackEvent, HttpContext, Task> MessageCallback {get; set;}

    private IDictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> _callCallbackDictionary;

    /// <summary>
    /// Handler of call callback events as dictionary
    /// </summary>
    public IDictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> CallCallbackDictionary 
    {
      get
      {
        return _callCallbackDictionary;
      } 
      set 
      {
        _callCallbackDictionary = value;
        CallCallback = BuildHandler(value);
      }
    }

    private IDictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> _messageCallbackDictionary;

    /// <summary>
    /// Handler of message callback events as dictionary
    /// </summary>
    public IDictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> MessageCallbackDictionary 
    {
      get
      {
        return _messageCallbackDictionary;
      } 
      set 
      {
        _messageCallbackDictionary = value;
        MessageCallback = BuildHandler(value);
      }
    }

    private Func<CallbackEvent, HttpContext, Task> BuildHandler(IDictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> dictionary)
    {
      return async (eventData, context) => {
        dictionary = dictionary ?? new Dictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>>();
        if (dictionary.TryGetValue(eventData.EventType, out var handler)) 
        {
          if (handler != null)
          {
            await handler(eventData, context);
          }
        }
      };
    }
  }

  /// <summary>
  /// Options for allocated phone number
  /// </summary>
  public class PhoneNumberOptions
  {
    /// <summary>
    /// Name of phone number
    /// </summary>
    /// <returns></returns>
    public string Name {get; set;}
    
    /// <summary>
    /// Options for local phone number. If missing toll free number will be allocated.
    /// </summary>
    public LocalNumberQueryForOrder LocalNumberQueryForOrder {get; set;}
  }
}
