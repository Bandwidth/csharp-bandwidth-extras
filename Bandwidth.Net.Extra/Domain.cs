using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bandwidth.Net;
using Bandwidth.Net.Api;

namespace Bandwidth.Net.Extra
{
    /// <summary>
    /// Extensions for IDomain
    /// </summary>
    public static class DomainExtensions 
    {
      /// <summary>
      /// Return all domain by 1 web request
      /// </summary>
      /// <param name="domain">IDomain instance</param>
      /// <returns>Domain instances</returns>
      public static IEnumerable<Domain> ListAll(this IDomain domain) 
      {
        return domain.List(new DomainQuery{Size = 100});
      }

      /// <summary>
      /// Return Domain instance by name
      /// </summary>
      /// <param name="domain">IDomain instance</param>
      /// <param name="name">Domain name</param>
      /// <returns>Domain instance or null</returns>
      public static Domain GetByName(this IDomain domain, string name) 
      {
        return domain.ListAll().FirstOrDefault(a => a.Name == name);
      }

      /// <summary>
      /// Return Domain instance by name or create it if it is missing
      /// </summary>
      /// <param name="domain">IDomain instance</param>
      /// <param name="name">Domain name</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Domain Id (existing or created)</returns>
      public static async Task<string> GetOrCreateAsync(this IDomain domain, string name, CancellationToken? cancellationToken = null) 
      {
        var existingDomain = domain.GetByName(name);
        if (existingDomain != null) 
        {
          return existingDomain.Id;
        }
        return await domain.CreateAsync(new CreateDomainData{
          Name = name
        }, cancellationToken);
      }
    }
}
