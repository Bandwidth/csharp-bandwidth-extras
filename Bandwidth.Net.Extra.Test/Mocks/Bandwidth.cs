using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LightMock;
using Bandwidth.Net.Api;
using System;

namespace Bandwidth.Net.Extra.Test.Mocks
{
  public class MockApplication : IApplication
  {
    private readonly IInvocationContext<IApplication> _context;

    public MockApplication(IInvocationContext<IApplication> context)
    {
      _context = context;
    }
    IEnumerable<Application> IApplication.List(ApplicationQuery query, CancellationToken? cancellationToken)
    {
        return _context.Invoke(f => f.List(query, cancellationToken));
    }

    Task<string> IApplication.CreateAsync(CreateApplicationData data, CancellationToken? cancellationToken)
    {
        return _context.Invoke(f => f.CreateAsync(data, cancellationToken));
    }

    Task<Application> IApplication.GetAsync(string applicationId, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task IApplication.UpdateAsync(string applicationId, UpdateApplicationData data, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task IApplication.DeleteAsync(string applicationId, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }
  }

  public class MockDomain : IDomain
  {
    private readonly IInvocationContext<IDomain> _context;

    public MockDomain(IInvocationContext<IDomain> context)
    {
      _context = context;
    }

    public Task<string> CreateAsync(CreateDomainData data, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.CreateAsync(data, cancellationToken));
    }

    public Task DeleteAsync(string domainId, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Domain> List(DomainQuery query = null, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.List(query, cancellationToken));
    }
  }

  public class MockEndpoint : IEndpoint
  {
    private readonly IInvocationContext<IEndpoint> _context;

    public MockEndpoint(IInvocationContext<IEndpoint> context)
    {
      _context = context;
    }

    public Task<string> CreateAsync(CreateEndpointData data, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.CreateAsync(data, cancellationToken));
    }

    public Task<EndpointAuthToken> CreateAuthTokenAsync(string domainId, string endpointId, CreateAuthTokenData data = null, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string domainId, string endpointId, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }

    public Task<Endpoint> GetAsync(string domainId, string endpointId, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.GetAsync(domainId, endpointId, cancellationToken));
    }

    public IEnumerable<Endpoint> List(string domainId, EndpointQuery query = null, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.List(domainId, query, cancellationToken));
    }

    public Task UpdateAsync(string domainId, string endpointId, UpdateEndpointData data, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }
  }

  public class MockPhoneNumber : IPhoneNumber
  {
    private readonly IInvocationContext<IPhoneNumber> _context;

    public MockPhoneNumber(IInvocationContext<IPhoneNumber> context)
    {
      _context = context;
    }

    public Task<string> CreateAsync(CreatePhoneNumberData data, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string phoneNumberId, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }

    public Task<PhoneNumber> GetAsync(string phoneNumberId, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }

    public IEnumerable<PhoneNumber> List(PhoneNumberQuery query = null, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.List(query, cancellationToken));
    }

    public Task UpdateAsync(string phoneNumberId, UpdatePhoneNumberData data, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.UpdateAsync(phoneNumberId, data, cancellationToken));
    }
  }

  public class MockAvailableNumber : IAvailableNumber
  {
    private readonly IInvocationContext<IAvailableNumber> _context;

    public MockAvailableNumber(IInvocationContext<IAvailableNumber> context)
    {
      _context = context;
    }

    public Task<OrderedNumber[]> SearchAndOrderLocalAsync(LocalNumberQueryForOrder query, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.SearchAndOrderLocalAsync(query, cancellationToken));
    }

    public Task<OrderedNumber[]> SearchAndOrderTollFreeAsync(TollFreeNumberQueryForOrder query, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        return _context.Invoke(f => f.SearchAndOrderTollFreeAsync(query, cancellationToken));
    }

    public Task<AvailableNumber[]> SearchLocalAsync(LocalNumberQuery query, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }

    public Task<AvailableNumber[]> SearchTollFreeAsync(TollFreeNumberQuery query, CancellationToken? cancellationToken = default(CancellationToken?))
    {
        throw new NotImplementedException();
    }
  }
}
