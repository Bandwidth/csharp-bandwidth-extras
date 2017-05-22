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
}
