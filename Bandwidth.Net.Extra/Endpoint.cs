using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bandwidth.Net;
using Bandwidth.Net.Api;

namespace Bandwidth.Net.Extra
{
    /// <summary>
    /// Extensions for IEndpoint
    /// </summary>
    public static class EndpointExtensions 
    {
      /// <summary>
      /// Return all domain enpoints by 1 web request
      /// </summary>
      /// <param name="endpoint">IEndpoint instance</param>
      /// <param name="domainId">Domain Id</param>
      /// <returns>Endpoint instances</returns>
      public static IEnumerable<Endpoint> ListAll(this IEndpoint endpoint, string domainId) 
      {
        return endpoint.List(domainId, new EndpointQuery{Size = 1000});
      }

      /// <summary>
      /// Return endpoint by name
      /// </summary>
      /// <param name="endpoint">IEndpoint instance</param>
      /// <param name="domainId">Domain Id</param>
      /// <param name="name">Name of enpoint</param>
      /// <returns>Endpoint instance or null</returns>
      public static Endpoint GetByName(this IEndpoint endpoint, string domainId, string name) 
      {
        return endpoint.ListAll(domainId).FirstOrDefault(a => a.Name == name);
      }

      /// <summary>
      /// Return Endpoint instance by name or create it if it is missing
      /// </summary>
      /// <param name="endpoint">IEndpoint instance</param>
      /// <param name="data">Options to create an endpoint (property Name is required and  used to search enpoint too)</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Endpoint instance (existing or created)</returns>
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

      /// <summary>
      /// Return Endpoint instance by name or create it if it is missing
      /// </summary>
      /// <param name="endpoint">IEndpoint instance</param>
      /// <param name="applicationId">Application Id</param>
      /// <param name="domainId">Domain Id</param>
      /// <param name="name">Name of endpoint</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Endpoint instance (existing or created)</returns>
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
