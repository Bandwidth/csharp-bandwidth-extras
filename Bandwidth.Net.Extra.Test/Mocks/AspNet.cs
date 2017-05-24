using LightMock;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http.Authentication;
using System.Security.Claims;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace Bandwidth.Net.Extra.Test.Mocks
{
  public class MockApplicationBuilder : IApplicationBuilder
  {
    public IServiceProvider ApplicationServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IFeatureCollection ServerFeatures => throw new NotImplementedException();

    public IDictionary<string, object> Properties => throw new NotImplementedException();

    public Func<RequestDelegate, RequestDelegate> Middleware { get; private set; }

    public RequestDelegate Build()
    {
        return Middleware(c => Task.FromResult(0));
    }

    public IApplicationBuilder New()
    {
        return new MockApplicationBuilder();
    }

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        Middleware = middleware;
        return this;
    }
  }

  public class MockServiceCollection : IServiceCollection
  {
    private readonly IInvocationContext<IServiceCollection> _context;
    private readonly List<ServiceDescriptor> _list = new List<ServiceDescriptor>();

    public MockServiceCollection(IInvocationContext<IServiceCollection> context)
    {
      _context = context;
    }

    public ServiceDescriptor this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int Count => _list.Count;
 
    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(ServiceDescriptor item)
    {
        _list.Add(item);
        _context.Invoke(f => f.Add(item));
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(ServiceDescriptor item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<ServiceDescriptor> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public int IndexOf(ServiceDescriptor item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        throw new NotImplementedException();
    }

    public bool Remove(ServiceDescriptor item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
  }

  public class MockMemoryCache : IMemoryCache
  {
    private readonly IInvocationContext<IMemoryCache> _context;

    public MockMemoryCache(IInvocationContext<IMemoryCache> context)
    {
      _context = context;
    }

    public ICacheEntry CreateEntry(object key)
    {
        return _context.Invoke(m => m.CreateEntry(key)) ?? new MockCacheEntry(key.ToString());
    }

    public void Dispose()
    {
    }

    public void Remove(object key)
    {
        _context.Invoke(m => m.Remove(key));
    }

    public bool TryGetValue(object key, out object value)
    {
        var result = (object)null;
        var r =  _context.Invoke(m => m.TryGetValue(key, out result));
        value = result;
        return r;
    }
  }

  public class MockCacheEntry : ICacheEntry
  {
    private readonly string _key;

    public MockCacheEntry(string key)
    {
      _key = key;
    }

    public object Key => _key;

    public object Value { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }

    public IList<IChangeToken> ExpirationTokens => new IChangeToken[0];

    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => new PostEvictionCallbackRegistration[0];

    public CacheItemPriority Priority { get; set; }

    public void Dispose()
    {
    }
  }

    public class MockHttpContext : HttpContext, IServiceProvider, IDisposable
    {
        public MockHttpContext()
        {
          Items = new Dictionary<object, object>();
          RequestServices = this;
        }

        private readonly MockHttpRequest _request = new MockHttpRequest();
        private readonly MockHttpResponse _response = new MockHttpResponse();

        public Dictionary<Type, object> Services = new Dictionary<Type, object>();
        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request => _request;

        public override HttpResponse Response => _response;

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override AuthenticationManager Authentication => throw new NotImplementedException();

        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object> Items { get; set; }
        public override IServiceProvider RequestServices { get; set; }
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; }
        public override ISession Session { get; set; }

        public override void Abort()
        {
        }

        public object GetService(Type serviceType)
        {
            var res = (object)null;
            Services.TryGetValue(serviceType, out res);
            return res;
        }

        public void AddService<T>(object instance)
        {
          Services[typeof(T)] = instance;
        }

        public void Dispose()
        {
            _response.Dispose();
            if (_request.Body != null)
            {
              _request.Body.Dispose();
            }
        }
    }

    public class MockHttpRequest : HttpRequest
    {
        public MockHttpRequest()
        {
          Host = new HostString("localhost");
        }
        public override HttpContext HttpContext => throw new NotImplementedException();

        public override string Method { get; set; }
        public override string Scheme { get; set; }
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; }
        public override string Protocol { get; set; }

        public override IHeaderDictionary Headers => throw new NotImplementedException();

        public override IRequestCookieCollection Cookies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override Stream Body { get; set; }

        public override bool HasFormContentType => throw new NotImplementedException();

        public override IFormCollection Form { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }

    public class MockHttpResponse : HttpResponse, IDisposable
    {
        public MockHttpResponse()
        {
          Body = new MemoryStream();
        }
        public override HttpContext HttpContext => throw new NotImplementedException();

        public override int StatusCode { get; set; }

        public override IHeaderDictionary Headers => throw new NotImplementedException();

        public override Stream Body { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }

        public override IResponseCookies Cookies => throw new NotImplementedException();

        public override bool HasStarted => throw new NotImplementedException();

        public void Dispose()
        {
            Body.Dispose();
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public override void Redirect(string location, bool permanent)
        {
        }
    }
}
