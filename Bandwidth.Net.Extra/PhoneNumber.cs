using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bandwidth.Net;
using Bandwidth.Net.Api;

namespace Bandwidth.Net.Extra
{
    public static class PhoneNumberExtensions 
    {
      public static IEnumerable<PhoneNumber> ListAllForApplication(this IPhoneNumber phoneNumber, string applicationId) 
      {
        return phoneNumber.List(new PhoneNumberQuery{Size = 1000, ApplicationId = applicationId});
      }

      public static PhoneNumber GetByName(this IPhoneNumber phoneNumber, string applicationId, string name = null) 
      {
        return phoneNumber.List(new PhoneNumberQuery{Size = 1000, ApplicationId = applicationId, Name = name}).FirstOrDefault();
      }

      public static async Task<string> CreateLocalAsync(this IPhoneNumber phoneNumber, IAvailableNumber availableNumber, string applicationId, LocalNumberQueryForOrder query = null, string name = null, CancellationToken? cancellationToken = null) 
      {
        query = query ?? new LocalNumberQueryForOrder();
        query.Quantity = 1;
        var result = (await availableNumber.SearchAndOrderLocalAsync(query, cancellationToken)).First();
        await phoneNumber.UpdateAsync(result.Id, new UpdatePhoneNumberData
        {
          ApplicationId = applicationId,
          Name = name
        }, cancellationToken);
        return result.Number;
      }

      public static async Task<string> CreateTollFreeAsync(this IPhoneNumber phoneNumber, IAvailableNumber availableNumber, string applicationId, string name = null, CancellationToken? cancellationToken = null) 
      {
        var query = new TollFreeNumberQuery {Quantity = 1};
        var result = (await availableNumber.SearchAndOrderTollFreeAsync(query, cancellationToken)).First();
        await phoneNumber.UpdateAsync(result.Id, new UpdatePhoneNumberData
        {
          ApplicationId = applicationId,
          Name = name
        }, cancellationToken);
        return result.Number;
      }

      public static async Task<string> GetOrCreateLocalAsync(this IPhoneNumber phoneNumber, IAvailableNumber availableNumber, string applicationId, LocalNumberQueryForOrder query = null, string name = null, CancellationToken? cancellationToken = null)
      {
        var existingNumber = phoneNumber.GetByName(applicationId, name);
        if (existingNumber != null) 
        {
          return existingNumber.Number;
        }
        return await phoneNumber.CreateLocalAsync(availableNumber, applicationId, query, name, cancellationToken);
      }
      public static async Task<string> GetOrCreateTollFreeAsync(this IPhoneNumber phoneNumber, IAvailableNumber availableNumber, string applicationId, string name = null, CancellationToken? cancellationToken = null)
      {
        var existingNumber = phoneNumber.GetByName(applicationId, name);
        if (existingNumber != null) 
        {
          return existingNumber.Number;
        }
        return await phoneNumber.CreateTollFreeAsync(availableNumber, applicationId, name, cancellationToken);
      }
    }
}
