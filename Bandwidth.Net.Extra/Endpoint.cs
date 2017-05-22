using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bandwidth.Net;
using Bandwidth.Net.Api;

namespace Bandwidth.Net.Extra
{
    public static class EndpointExtensions 
    {
      public static IEnumerable<Endpoint> ListAll(this IEndpoint endpoint, string domainId) 
      {
        return endpoint.List(domainId, new EndpointQuery{Size = 1000});
      }

      public static Endpoint GetByName(this IEndpoint endpoint, string domainId, string name) 
      {
        return endpoint.ListAll(domainId).FirstOrDefault(a => a.Name == name);
      }

      public static async Task<Endpoint> GetOrCreateAsync(this IEndpoint endpoint, CreateEndpointData data, CancellationToken? cancellationToken = null) 
      {
        var existingEndpoint = endpoint.GetByName(data.DomainId, data.Name);
        if (existingEndpoint != null) 
        {
          return existingEndpoint;
        }
        var id = await endpoint.CreateAsync(data, cancellationToken);
        return await endpoint.GetAsync(data.DomainId, id);
      }

      public static async Task<Endpoint> GetOrCreateAsync(this IEndpoint endpoint, string applicationId, string domainId, string name, CancellationToken? cancellationToken = null) 
      {
        var existingEndpoint = endpoint.GetByName(domainId, name);
        if (existingEndpoint != null) 
        {
          return existingEndpoint;
        }
        var id = await endpoint.CreateAsync(new CreateEndpointData{
          DomainId = domainId,
          ApplicationId = applicationId,
          Name = name,
          Credentials = new CreateEndpointCredentials{Password = new Guid().ToString("N")}
        }, cancellationToken);
        return await endpoint.GetAsync(domainId, id);
      }

    }
}
