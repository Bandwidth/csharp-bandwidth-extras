using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bandwidth.Net;
using Bandwidth.Net.Api;

namespace Bandwidth.Net.Extra
{
    /// <summary>
    /// Extensions for IApplication
    /// </summary>
    public static class ApplicationExtensions 
    {
      /// <summary>
      /// Route's path g web application to handle call callbacks events
      /// </summary>
      public const string CallCallbackPath = "/bandwidth/callback/call";
      
      /// <summary>
      /// Route's path g web application to handle message callbacks events
      /// </summary>
      public const string MessageCallbackPath = "/bandwidth/callback/message";
      
      /// <summary>
      /// Return all apllications by 1 web request
      /// </summary>
      /// <param name="application">IApplication instance</param>
      /// <returns>Applications list</returns>
      public static IEnumerable<Application> ListAll(this IApplication application) 
      {
        return application.List(new ApplicationQuery{Size = 1000});
      }

      /// <summary>
      /// Return Application instance by name
      /// </summary>
      /// <param name="application">IApplication instance</param>
      /// <param name="name">Name of application to search</param>
      /// <returns>Application instance or null</returns>
      public static Application GetByName(this IApplication application, string name) 
      {
        return application.ListAll().FirstOrDefault(a => a.Name == name);
      }

      /// <summary>
      /// Return Application instance by name or create it if it is missing
      /// </summary>
      /// <param name="application">IApplication instance</param>
      /// <param name="data">Options to create application (property Name is required and  used to search application too)</param>
      /// <param name="host">Host of the web application</param>
      /// <param name="useHttps">Use or not HTTPS for Bandwidth event callbacks</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Id of existing (or created) application</returns>
      public static async Task<string> GetOrCreateAsync(this IApplication application, CreateApplicationData data, string host, bool useHttps = true, CancellationToken? cancellationToken = null) 
      {
        data.Name = $"{data.Name} on {host}";
        var app = application.GetByName(data.Name);
        if (app != null) 
        {
          return app.Id;
        }
        var baseUrl = $"http{(useHttps ? "s" : "")}://{host}";
        data.IncomingCallUrl = data.IncomingCallUrl ?? $"{baseUrl}{CallCallbackPath}";
        data.IncomingMessageUrl = data.IncomingMessageUrl ?? $"{baseUrl}{MessageCallbackPath}";
        data.AutoAnswer = data.AutoAnswer ?? true;
        data.CallbackHttpMethod = data.CallbackHttpMethod ?? CallbackHttpMethod.Post;
        return await application.CreateAsync(data, cancellationToken);
      }

      /// <summary>
      /// Return Application instance by name or create it if it is missing
      /// </summary>
      /// <param name="application">IApplication instance</param>
      /// <param name="name">Name of application</param>
      /// <param name="host">Host of the web application</param>
      /// <param name="useHttps">Use or not HTTPS for Bandwidth event callbacks</param>
      /// <param name="cancellationToken">Cancellation token</param>
      /// <returns>Id of existing (or created) application</returns>
      public static Task<string> GetOrCreateAsync(this IApplication application, string  name, string host, bool useHttps = true, CancellationToken? cancellationToken = null) 
      {
        return application.GetOrCreateAsync(new CreateApplicationData{Name = name}, host, useHttps, cancellationToken);
      }
    }
}
