using System;
using System.Linq;
using Xunit;
using Bandwidth.Net.Api;
using Bandwidth.Net.Extra.Test.Mocks;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using LightMock;
using Microsoft.Extensions.Caching.Memory;
using System.IO;

namespace Bandwidth.Net.Extra.Test.Middlewares
{
    public class ApplicationBuilderTests
    {
        private static (RequestDelegate, HttpContext, MockContexts) GetMiddleware(BandwidthOptions options, Action<MockHttpContext> buildHttpContext = null)
        {
          var builder = new MockApplicationBuilder();
          builder.UseBandwidth(options);
          Assert.NotNull(builder.Middleware);
          var context = new MockHttpContext();
          var mockContexts = new MockContexts();
          context.AddService<IMemoryCache>(new MockMemoryCache(mockContexts.MemoryCache));
          context.AddService<IApplication>(new MockApplication(mockContexts.Application));
          context.AddService<IPhoneNumber>(new MockPhoneNumber(mockContexts.PhoneNumber));
          context.AddService<IAvailableNumber>(new MockAvailableNumber(mockContexts.AvailableNumber));
          context.AddService<IDomain>(new MockDomain(mockContexts.Domain));
          if (buildHttpContext != null)
          {
            buildHttpContext(context);
          }
          return (builder.Middleware(c => Task.FromResult(0)), context, mockContexts);
        }

        private static void SetJsonBody(HttpRequest request, string json)
        {
          request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
          request.ContentType = "application/json";
        }
        
        [Fact]
        public async void UseBandwidthTest()
        {
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test"
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test on localhost"
            }
          });
          await middleware(context);
          Assert.Equal("appId", context.Items["ApplicationId"]);
          Assert.False(context.Items.ContainsKey("PhoneNumber"));
        }

        [Fact]
        public async void UseBandwidthTest2()
        {
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test2",
            PhoneNumber = new PhoneNumberOptions 
            {
              Name = "Service Number"
            }
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test2 on localhost"
            }
          });
          mockContexts.PhoneNumber.Arrange(p => p.List(The<PhoneNumberQuery>.IsAnyValue, null)).Returns(new[]{
            new PhoneNumber {
              Name = "Service Number",
              Number = "+1234567890"
            }
          });
          await middleware(context);
          Assert.Equal("appId", context.Items["ApplicationId"]);
          Assert.Equal("+1234567890", context.Items["PhoneNumber"]);
        }

        [Fact]
        public async void UseBandwidthTest3()
        {
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test3",
            PhoneNumber = new PhoneNumberOptions 
            {
              Name = "Service Number",
              LocalNumberQueryForOrder = new LocalNumberQueryForOrder
              {
                AreaCode = "910"
              }
            }
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test3 on localhost"
            }
          });
          mockContexts.PhoneNumber.Arrange(p => p.List(The<PhoneNumberQuery>.IsAnyValue, null)).Returns(new PhoneNumber[0]);
          mockContexts.AvailableNumber.Arrange(p => p.SearchAndOrderLocalAsync(The<LocalNumberQueryForOrder>.Is(q => q.AreaCode == "910"), null)).Returns(Task.FromResult(new[]
          {
            new OrderedNumber()
            {
              Location = "http://localhost/phoneNumberId1",
              Number = "+1234567890"
            }
          }));
          mockContexts.PhoneNumber
            .Arrange(p => p.UpdateAsync("phoneNumberId1", The<UpdatePhoneNumberData>.Is(d => d.ApplicationId == "appId" && d.Name == "Service Number"), null))
            .Returns(Task.FromResult(0));
          await middleware(context); 
          Assert.Equal("appId", context.Items["ApplicationId"]);
          Assert.Equal("+1234567890", context.Items["PhoneNumber"]);
        }

        [Fact]
        public async void UseBandwidthTest4()
        {
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test4",
            DomainName = "test"
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test4 on localhost"
            }
          });
          mockContexts.Domain.Arrange(p => p.List(The<DomainQuery>.IsAnyValue, null)).Returns(new []
          {
            new Domain
            {
              Id = "domainId",
              Name = "test"
            }
          });
          await middleware(context); 
          Assert.Equal("appId", context.Items["ApplicationId"]);
          Assert.Equal("domainId", context.Items["DomainId"]);
        }
        
        [Fact]
        public async void UseBandwidthTest5()
        {
          var called = false;
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test5",
            CallCallback = (eventData, ctx) => {
              called = true;
              Assert.Equal(CallbackEventType.Answer, eventData.EventType);
              return Task.FromResult(0);
            }
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test5 on localhost"
            }
          });
          context.Request.Method = HttpMethods.Post;
          context.Request.Path = ApplicationExtensions.CallCallbackPath;
          SetJsonBody(context.Request, "{\"eventType\": \"answer\"}");
          using((IDisposable)context)
          {
            await middleware(context); 
          }
          Assert.True(called);
        }

        [Fact]
        public async void UseBandwidthTest6()
        {
          var called = false;
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test6",
            CallCallbackDictionary = new Dictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> {
              {CallbackEventType.Answer, (eventData, ctx) => {
                called = true;
                return Task.FromResult(0);
              }}
            }
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test6 on localhost"
            }
          });
          context.Request.Method = HttpMethods.Post;
          context.Request.Path = ApplicationExtensions.CallCallbackPath;
          SetJsonBody(context.Request, "{\"eventType\": \"answer\"}");
          using((IDisposable)context)
          {
            await middleware(context); 
          }
          Assert.True(called);
        }

        [Fact]
        public async void UseBandwidthTest7()
        {
          var called = false;
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test7",
            MessageCallback = (eventData, ctx) => {
              called = true;
              Assert.Equal(CallbackEventType.Sms, eventData.EventType);
              return Task.FromResult(0);
            }
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test7 on localhost"
            }
          });
          context.Request.Method = HttpMethods.Post;
          context.Request.Path = ApplicationExtensions.MessageCallbackPath;
          SetJsonBody(context.Request, "{\"eventType\": \"sms\"}");
          using((IDisposable)context)
          {
            await middleware(context); 
          }
          Assert.True(called);
        }

        [Fact]
        public async void UseBandwidthTest8()
        {
          var called = false;
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test8",
            MessageCallbackDictionary = new Dictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> {
              {CallbackEventType.Sms, (eventData, ctx) => {
                called = true;
                return Task.FromResult(0);
              }}
            }
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test8 on localhost"
            }
          });
          context.Request.Method = HttpMethods.Post;
          context.Request.Path = ApplicationExtensions.MessageCallbackPath;
          SetJsonBody(context.Request, "{\"eventType\": \"sms\"}");
          using((IDisposable)context)
          {
            await middleware(context); 
          }
          Assert.True(called);
        }

        [Fact]
        public async void UseBandwidthTest9()
        {
          var called = false;
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test9",
            MessageCallbackDictionary = new Dictionary<CallbackEventType, Func<CallbackEvent, HttpContext, Task>> {
              {CallbackEventType.Sms, (eventData, ctx) => {
                called = true;
                return Task.FromResult(0);
              }}
            }
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test9 on localhost"
            }
          });
          context.Request.Method = HttpMethods.Post;
          context.Request.Path = ApplicationExtensions.MessageCallbackPath;
          SetJsonBody(context.Request, "{\"eventType\": \"mms\"}");
          using((IDisposable)context)
          {
            await middleware(context); 
          }
          Assert.False(called);
        }

        [Fact]
        public async void UseBandwidthTest10()
        {
          var (middleware, context, mockContexts) = GetMiddleware(new BandwidthOptions
          {
            ApplicationName = "Test10",
            MessageCallback = (data, ctx) => throw new Exception()
          });
          mockContexts.Application.Arrange(a => a.List(The<ApplicationQuery>.IsAnyValue, null)).Returns(new[]
          {
            new Application
            {
              Id = "appId",
              Name = "Test10 on localhost"
            }
          });
          context.Request.Method = HttpMethods.Post;
          context.Request.Path = ApplicationExtensions.MessageCallbackPath;
          SetJsonBody(context.Request, "{\"eventType\": \"mms\"}");
          using((IDisposable)context)
          {
            await middleware(context);
          }
        }

        [Fact]
        public void GetServiceTest()
        {
          var context = new MockContext<IServiceProvider>();
          var serviceProvider = new MockServiceProvider(context);
          context.Arrange(s => s.GetService(typeof(int))).Returns(1);
          Assert.Equal(1, serviceProvider.GetService<int>());
        }

        [Fact]
        public void GetRequestServiceTest()
        {
          var context = new MockContext<IServiceProvider>();
          var serviceProvider = new MockServiceProvider(context);
          var httpContext = new MockHttpContext
          {
            RequestServices = serviceProvider
          };
          context.Arrange(s => s.GetService(typeof(int))).Returns(10);
          Assert.Equal(10, httpContext.GetRequestService<int>());
        }

        [Fact]
        public void GetApplicationIdTest()
        {
          var httpContext = new MockHttpContext
          {
            Items = new Dictionary<object, object>{{"ApplicationId", "appId"}}
          };
          Assert.Equal("appId", httpContext.GetApplicationId());
        }

        [Fact]
        public void GetDomainIdTest()
        {
          var httpContext = new MockHttpContext
          {
            Items = new Dictionary<object, object>{{"DomainId", "domainId"}}
          };
          Assert.Equal("domainId", httpContext.GetDomainId());
        }

        [Fact]
        public void GetPhoneNumberTest()
        {
          var httpContext = new MockHttpContext
          {
            Items = new Dictionary<object, object>{{"PhoneNumber", "+1234567890"}}
          };
          Assert.Equal("+1234567890", httpContext.GetPhoneNumber());
        }

        public class MockContexts
        {
          public MockContext<IMemoryCache> MemoryCache = new MockContext<IMemoryCache>();
          public MockContext<IApplication> Application = new MockContext<IApplication>();
          public MockContext<IPhoneNumber> PhoneNumber = new MockContext<IPhoneNumber>();
          public MockContext<IAvailableNumber> AvailableNumber = new MockContext<IAvailableNumber>();
          public MockContext<IDomain> Domain = new MockContext<IDomain>();
        }
    }
}
