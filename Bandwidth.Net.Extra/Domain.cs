using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bandwidth.Net;
using Bandwidth.Net.Api;

namespace Bandwidth.Net.Extra
{
    public static class DomainExtensions 
    {
      public static IEnumerable<Domain> ListAll(this IDomain domain) 
      {
        return domain.List(new DomainQuery{Size = 100});
      }

      public static Domain GetByName(this IDomain domain, string name) 
      {
        return domain.ListAll().FirstOrDefault(a => a.Name == name);
      }

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
