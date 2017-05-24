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
    /// Extensions for IPhoneNumber
    /// </summary>
    public static class PhoneNumberExtensions 
    {
      /// <summary>
      /// Return all phone numbers asiigned to the application by 1 web request
      /// </summary>
      /// <param name="phoneNumber">IPhoneNumber instance</param>
      /// <param name="applicationId">Application Id</param>
      /// <returns>List of numbers</returns>
      public static IEnumerable<PhoneNumber> ListAllForApplication(this IPhoneNumber phoneNumber, string applicationId) 
      {
        return phoneNumber.List(new PhoneNumberQuery{Size = 1000, ApplicationId = applicationId});
      }

      /// <summary>
      /// Get phone number assigned to the application by name
      /// </summary>
      /// <param name="phoneNumber">IPhoneNumber instance</param>
      /// <param name="applicationId">Application Id</param>
      /// <param name="name">Name of phone number. If Name is omit first application number will be returned</param>
      /// <returns>PhoneNumber instance or null</returns>
      public static PhoneNumber GetByName(this IPhoneNumber phoneNumber, string applicationId, string name = null) 
      {
        return phoneNumber.List(new PhoneNumberQuery{Size = 1000, ApplicationId = applicationId, Name = name}).FirstOrDefault();
      }

      /// <summary>
      /// Allocate local phone number and assign it to the application
      /// </summary>
      /// <param name="phoneNumber">IPhoneNumber instance</param>
      /// <param name="availableNumber">IAvailableNumber instance</param>
      /// <param name="applicationId">Application Id</param>
      /// <param name="query">Optioons to allocate number</param>
      /// <param name="name">Name of allocated phone number</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Created phone number</returns>
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

      /// <summary>
      /// Allocate toll free number and assign it to the application
      /// </summary>
      /// <param name="phoneNumber">IPhoneNumber instance</param>
      /// <param name="availableNumber">IAvailableNumber instance</param>
      /// <param name="applicationId">Application Id</param>
      /// <param name="name">Name of allocated phone number</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Created phone number</returns>
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

      /// <summary>
      /// Get local phone number assigned to the applcation by name (or reserve it if need)
      /// </summary>
      /// <param name="phoneNumber">IPhoneNumber instance</param>
      /// <param name="availableNumber">IAvailableNumber instance</param>
      /// <param name="applicationId">Application Id</param>
      /// <param name="query">Optioons to allocate number</param>
      /// <param name="name">Name of allocated phone number</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Phone number</returns>
      public static async Task<string> GetOrCreateLocalAsync(this IPhoneNumber phoneNumber, IAvailableNumber availableNumber, string applicationId, LocalNumberQueryForOrder query = null, string name = null, CancellationToken? cancellationToken = null)
      {
        var existingNumber = phoneNumber.GetByName(applicationId, name);
        if (existingNumber != null) 
        {
          return existingNumber.Number;
        }
        return await phoneNumber.CreateLocalAsync(availableNumber, applicationId, query, name, cancellationToken);
      }

      /// <summary>
      /// Get toll free phone number assigned to the applcation by name (or reserve it if need)
      /// </summary>
      /// <param name="phoneNumber">IPhoneNumber instance</param>
      /// <param name="availableNumber">IAvailableNumber instance</param>
      /// <param name="applicationId">Application Id</param>
      /// <param name="name">Name of allocated phone number</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Phone number</returns>
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
