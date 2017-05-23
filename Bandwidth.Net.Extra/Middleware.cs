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
  public static class AspNetCoreExtensions 
  {
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

    internal static void AddRouteHandler(this IApplicationBuilder builder, string route, Func<CallbackEvent, HttpContext, Task> handler) 
    {
      builder.Map(route, b => {
        b.Use(async (context, next) => {
          if (context.Request.Method != HttpMethods.Post || (context.Request.ContentType ?? "").Contains("/json"))
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

    internal static Task<T> CachedCall<T>(this IMemoryCache cache, string key, Func<Task<T>> func)
    {
      return cache.GetOrCreateAsync(key, e => {
        e.Priority = CacheItemPriority.NeverRemove;
        return func();
      });
    }

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

  public class BandwidthAuthData
  {
    public string UserId {get; set;}

    public string ApiToken {get; set;}

    public string ApiSecret {get; set;}
  }

  public class BandwidthOptions
  {
    public string ApplicationName {get; set;}

    public PhoneNumberOptions PhoneNumber {get; set;}

    public string DomainName {get; set;}

    public Func<CallbackEvent, HttpContext, Task> CallCallback {get; set;}
    
    public Func<CallbackEvent, HttpContext, Task> MessageCallback {get; set;}

    private IDictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> _callCallbackDictionary;
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

  public class PhoneNumberOptions
  {
    public string Name {get; set;}
    public LocalNumberQueryForOrder LocalNumberQueryForOrder {get; set;}
  }
}
